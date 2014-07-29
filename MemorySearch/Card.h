#include <string>

#pragma once

#ifdef MEMFUNCSDLLL_EXPORTS
#define MEMFUNCSDLL_API __declspec(dllexport) 
#else
#define MEMFUNCSDLL_API __declspec(dllimport) 
#endif

MEMFUNCSDLL_API struct Card
{
	std::string name;
	int id;
	std::string cardId;
	std::string zone;
	int zonePos;

	int memLocation;
};