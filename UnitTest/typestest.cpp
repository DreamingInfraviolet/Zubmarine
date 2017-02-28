#include "stdafx.h"
#include "CppUnitTest.h"
#include "ztypes.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace UnitTest
{
	TEST_CLASS(TypesTest)
	{
		template <class T>
		void assertSize(int expected)
		{
			Assert::AreEqual(static_cast<unsigned long long>(expected), sizeof(T) * 8);
		}

	public:

		TEST_METHOD(TestTypes)
		{
			assertSize<byte>(8);

			assertSize<i8>(8);
			assertSize<i16>(16);
			assertSize<i32>(32);
			assertSize<i64>(64);

			assertSize<u8>(8);
			assertSize<u16>(16);
			assertSize<u32>(32);
			assertSize<u64>(64);

			assertSize<float>(32);
			assertSize<double>(64);
		}
	};
}