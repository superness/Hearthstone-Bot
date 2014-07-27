// HearthstoneMemorySearchWrapper.h

#pragma once

#include <Card.h>
#include <HearthstoneMemoryReader.h>
#include "CardWrapper.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace HearthstoneMemorySearchCLR {

	public ref class HearthstoneMemorySearchWrapper
	{
	public:
		List<CardWrapper^>^ GetCardList();
	};
}
