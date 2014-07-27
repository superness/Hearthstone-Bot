// This is the main DLL file.

#include "stdafx.h"

#include "HearthstoneMemorySearchWrapper.h"
#include "CardWrapper.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;

namespace HearthstoneMemorySearchCLR
{
	List<CardWrapper^>^ HearthstoneMemorySearchWrapper::GetCardList()
	{
		std::vector<Card> cards = HearthstoneMemoryReader::GetCards();

		List<CardWrapper^>^ wrappedCards = gcnew List<CardWrapper^>();

		for (int i = 0; i < cards.size(); ++i)
		{
			CardWrapper^ wrapped = gcnew CardWrapper();
			wrapped->Init(cards[i]);

			wrappedCards->Add(wrapped);
		}

		return wrappedCards;
	}
}