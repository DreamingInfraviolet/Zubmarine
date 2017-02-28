#pragma once
#include "stdafx.h"
#include <functional>
#include "ztypes.h"
#include "export.h"
#include "ahardwarelayer.h"
#include "rate.h"

namespace zubmarine
{
	namespace hardware_layer
	{
		class DummyHardwareLayer : public AHardwareLayer
		{
		private:
			std::thread thread;
			std::atomic<bool> halt = false;
			util::Rate rate;

			static void threadFunction(DummyHardwareLayer& halt);

			DummyHardwareLayer(const DummyHardwareLayer&) = delete;
			DummyHardwareLayer(DummyHardwareLayer&&) = delete;

		public:

			ZUBMARINE_EXPORTS_API DummyHardwareLayer(UpdateCallbackType callback, const util::Rate& rate);
			ZUBMARINE_EXPORTS_API ~DummyHardwareLayer();

			ZUBMARINE_EXPORTS_API virtual i32 numberOfSlots() override;
		};
	}
}