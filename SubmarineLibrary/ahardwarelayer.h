#pragma once
#include <functional>
#include "ztypes.h"
#include "export.h"

namespace zubmarine
{
	namespace hardware_layer
	{
		class AHardwareLayer
		{
		public:
			/** The type of the callback function to call when a change of state occurs.
			  * The parameters are as follows:
			  * u32 slotId: The ID of the slot which is being updated
			  * byte* data: A pointer to the data being updated
			  * size_t len: The size of the data in bytes */
			using UpdateCallbackType = std::function<void(i32, byte*, size_t)>;

		
			/** Returns the number of input slots of the hardware machine.
			  * An input slot is analogous to a device (e.g. compass, barometer, etc.) */
			virtual i32 numberOfSlots() = 0;

			virtual void stop() = 0;

		protected:

			UpdateCallbackType updateCallback;

			/** Sets the update callback function to call when a change of state occurs.
			  * The callback must be thread safe as it may be called from another thread. */
			virtual void setUpdateCallback(UpdateCallbackType callback)
			{
				this->updateCallback = callback;
			}

			virtual ~AHardwareLayer() {}
		};
	}
}