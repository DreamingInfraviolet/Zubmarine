#pragma once
#include "stdafx.h"
#include "ahardwarelayer.h"
#include "ztypes.h"
#include "isensor.h"
#include "sensor_descriptor.h"
#include "hardware_event_queue.h"

namespace zubmarine
{
	namespace event_layer
	{
		using input::sensor::ISensor;

		/** An event queue that separates the reception of hardware data (which may need to be handled
		  * immediately) and the updating of registered sensors. */
		class EventLayer
		{
			// @TODO: Refactor this into a thread pool
			struct WorkerThread
			{
				EventLayer* eventLayer = nullptr;
				std::atomic<bool> hasWork = false;
				i32 slot;

			private:
				std::condition_variable condition;
				std::thread thread;
				std::mutex lock;

				WorkerThread(const WorkerThread&) = delete;
				WorkerThread(WorkerThread&&) = delete;

			public:

				std::function<void()> createFunction()
				{
					return [this]() { this->fn(); };
				}

				void start()
				{
					this->thread = std::thread([this]() { this->createFunction(); });
				}

				void stop()
				{
					this->halt = true;
				}

				void wake()
				{
					this->condition.notify_one();
				}

				void waitUntilStopped()
				{
					if (this->thread.joinable())
					{
						this->thread.join();
					}
				}

			private:
				std::atomic<bool> halt = false;

				void fn()
				{
					assert(this->eventLayer);

					while (!halt)
					{
						if (this->hasWork)
						{
							auto& queue = this->eventLayer->eventQueues[this->slot];

							// Lock parent since we will be modifying the event queue
							HardwareEventQueue::EventType event;
							{
								std::lock_guard<std::mutex> guard(this->eventLayer->lock);
								event = queue.pop();
							}

							// Update sensor
							this->eventLayer->sensors[this->slot]->update(std::move(event));

							// Check if there is more work to do
							if (queue.size() != 0)
							{
								this->hasWork = true;
							}
							else
							{
								this->hasWork = false;
							}
						}
						else
						{
							// Sleep until there is work!
							std::unique_lock<std::mutex> guard(this->lock);
							this->condition.wait(guard);
						}
					}
				}
			};

			
			/** Stores a copy of all data in queues. */
			std::vector<HardwareEventQueue> eventQueues;

			/** Mapping from slotId to a vector of sensors bound to it. */
			std::vector<std::shared_ptr<ISensor>> sensors;

			/** Local lock to avoid threading issues */
			std::mutex lock;

			/** Each slot gets a thread that's used to update associated sensors */
			std::vector<WorkerThread> workers;


			EventLayer(const EventLayer&) = delete;
			EventLayer(EventLayer&&) = delete;

			void updateCallback(i32 slotId, byte* data, size_t len)
			{
				// Assert that data is not null
				assert(data);

				// Append copy of the data to the appropriate queue
				{
					std::lock_guard<std::mutex> guard(this->lock);
					this->eventQueues[slotId].insertCopyOfData(data, len);
				}

				this->workers[slotId].hasWork = true;
				this->workers[slotId].wake();

			}

			void createWorkerThreads(i32 count)
			{
				// The worker threads must not be moved past this point to retain memory cohesion
				this->workers.resize(count);

				for (int i = 0; i < count; ++i)
				{
					auto& worker = this->workers[i];
					worker.eventLayer = this;
					worker.slot = i;
					worker.start();
				}
			}

			void haltWorkers()
			{
				// Notify all workers to stop
				for (auto& worker : this->workers)
				{
					worker.stop();
					worker.wake();
				}

				// Wait until all workers stopped
				for (auto& worker : this->workers)
				{
					worker.waitUntilStopped();
				}
			}

		public:

			EventLayer(i32 maxSlotCount)
			{
				// Initialise memory
				this->eventQueues = decltype(eventQueues)(maxSlotCount);
				this->sensors = decltype(sensors)(maxSlotCount);
				this->createWorkerThreads(maxSlotCount);
			}

			~EventLayer()
			{
				haltWorkers();
			}

			void registerSensor(const SensorDescriptor& descriptor)
			{
				// Assert basic expectations
				assert(descriptor.sensor);
				assert(descriptor.slot < this->sensors.size());

				// Assert that sensor does not already exist
				assert(this->sensors[descriptor.slot] == nullptr);

				// Insert sensor into slot
				this->sensors[descriptor.slot] = descriptor.sensor;

				// Set up event queue
				this->eventQueues[descriptor.slot].setMaxCapacity(descriptor.queueSize);
			}

			/** Creates an update callback to be called by the hardware layer */
			hardware_layer::AHardwareLayer::UpdateCallbackType createUpdateCallback()
			{
				return [this] (i32 a, byte* b, size_t c) { this->updateCallback(a, b, c); };
			}
		};
	}
}