#include "stdafx.h"
#include "CppUnitTest.h"
#include "zubmarine.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace UnitTest
{		
	TEST_CLASS(UnitTest1)
	{
	public:
		
		TEST_METHOD(TestMethod1)
		{
			zubmarine::sensor::CompassSensor sensor;
			Assert::AreEqual(3, sensor.x());
		}
	};
}