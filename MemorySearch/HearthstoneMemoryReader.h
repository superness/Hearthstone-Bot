#include <vector>
#include "Card.h"

#ifdef MEMFUNCSDLLL_EXPORTS
#define MEMFUNCSDLL_API __declspec(dllexport) 
#else
#define MEMFUNCSDLL_API __declspec(dllimport) 
#endif

MEMFUNCSDLL_API class HearthstoneMemoryReader
{
public:
	static MEMFUNCSDLL_API std::vector<Card> GetCards();
};