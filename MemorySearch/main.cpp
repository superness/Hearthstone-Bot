#include "HearthstoneMemoryReader.h"

#include <iostream>
#include <algorithm>    // std::sort

bool myfunction(Card i, Card j) { return (i.memLocation<j.memLocation); }

int main(void)
{
	//while (true)
	{
		HearthstoneMemoryReader reader = HearthstoneMemoryReader();

		std::vector<Card> cards = reader.GetCards();

		std::sort(cards.begin(), cards.end(), myfunction);

		for (int i = 0; i < cards.size(); ++i)
		{
			std::cout << "Cards: " << cards[i].name << std::endl;
			std::cout << "\t ID= " << cards[i].id << std::endl;
			std::cout << "\t CardID= " << cards[i].cardId << std::endl;
			std::cout << "\t Zone= " << cards[i].zone << std::endl;
			std::cout << "\t ZonePos= " << cards[i].zonePos << std::endl;
			std::cout << "\t MemLocation= " << cards[i].memLocation << std::endl;
			if (i > 0)
			{
				std::cout << "\t Offset= " << (cards[i].memLocation - cards[i-1].memLocation) << std::endl;
			}
		}
	}

	char c;
	std::cin >> c;
}