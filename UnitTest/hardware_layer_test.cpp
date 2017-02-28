#include "stdafx.h"
#include "ztypes.h"
#include "CppUnitTest.h"
#include "ahardwarelayer.h"
#include "dummy_hardware_layer.h"
#include "rate.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace zubmarine::hardware_layer;

namespace UnitTest
{
	TEST_CLASS(HardwareLayerTest)
	{


	public:

		TEST_METHOD(TestDummyLayer)
		{
			std::vector<std::pair<i32, std::vector<byte>>> vec;

			auto fn = [&vec](i32 slot, byte* data, size_t len)
			{
				std::vector<byte> subvec;

				if (data != nullptr && len != 0)
				{
					subvec.resize(len);
					memcpy(&subvec[0], data, len);
				}

				vec.emplace_back(slot, std::move(subvec));
			};

			DummyHardwareLayer layer(fn, util::Rate(30));
			std::this_thread::sleep_for(std::chrono::seconds(1));
			layer.stop();

			int iSlot = 0;

			for (auto& entry : vec)
			{
				auto slot = entry.first;
				auto data = entry.second;

				Assert::AreEqual(iSlot % layer.numberOfSlots(), slot);
				Assert::AreEqual(static_cast<size_t>(4), data.size());
				Assert::AreEqual(0, *reinterpret_cast<int*>(&data[0]));

				++iSlot;
			}
			
			Assert::IsTrue(iSlot > 15);

		}
	};
}