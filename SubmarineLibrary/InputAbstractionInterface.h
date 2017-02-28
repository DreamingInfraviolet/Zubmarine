#pragma once
#include "stdafx.h"
#include "iinput.h"
#include "export.h"

namespace zubmarine
{
	namespace input
	{
		namespace iai
		{
			class ZUBMARINE_EXPORTS_API InputAbstractionInterface
			{
			private:
				using Mapping = std::map<u32, std::vector<std::shared_ptr<IInput>>>;

			protected:
				InputAbstractionInterface() {}
				~InputAbstractionInterface() {}
			};
		}
	}
}