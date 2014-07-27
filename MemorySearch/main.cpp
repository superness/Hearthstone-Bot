#include "HearthstoneMemoryReader.h"

#include <iostream>

int main(void)
{
	HearthstoneMemoryReader reader = HearthstoneMemoryReader();

	std::vector<Card> cards = reader.GetCards();

	for (int i = 0; i < cards.size(); ++i)
	{
		std::cout << "Cards: " << cards[i].name << std::endl;
		std::cout << "\t ID= " << cards[i].id << std::endl;
		std::cout << "\t CardID= " << cards[i].cardId << std::endl;
		std::cout << "\t Zone= " << cards[i].zone << std::endl;
		std::cout << "\t ZonePos= " << cards[i].zonePos << std::endl;
	}

	char c;
	std::cin >> c;
}