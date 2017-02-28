#pragma once
#include "stdafx.h"

namespace zubmarine
{
	namespace event_layer
	{
		class HardwareEventQueue
		{
		public:
			using EventType = std::pair	<std::unique_ptr<byte[]>, size_t>;

			std::deque<EventType> queue;
			size_t queueCapacity;

			void fitToSize()
			{
				while (this->exceedsCapacity())
				{
					this->queue.pop_front();
				}
			}

		public:
			HardwareEventQueue(i32 queueCapacity)
				: queueCapacity(queueCapacity)
			{

			}

			void insertCopyOfData(byte* originalData, size_t len)
			{
				// Create copy of data
				auto copy = std::make_unique<byte[]>(len);
				std::memcpy(copy.get(), originalData, len);

				// Insert copy
				this->queue.emplace_back(std::move(copy), len);

				// Delete front if needed
				// We keep queueSize + 1 elements, as queueSize of 0 indicates that no history is kept.
				this->fitToSize();
			}

			void setMaxCapacity(size_t size)
			{
				this->queueCapacity = size;
				this->fitToSize();
			}

			bool exceedsCapacity() const
			{
				return this->queue.size() > this->queueCapacity + 1;
			}

			auto size() const
			{
				return this->queue.size();
			}

			auto capacity() const
			{
				return this->queueCapacity;
			}

			auto pop()
			{
				auto element = std::move(this->queue.front());
				this->queue.pop_front();
				return element;
			}
		};
	}
}