#include "HearthstoneMemoryReader.h"
#include <iostream>
#include <Windows.h>
#include <tchar.h>
#include <psapi.h>
#include <algorithm>
#include <string>
#include <vector>
#include <sstream>
#include <iterator>

std::vector<std::string> &split(const std::string &s, char delim, std::vector<std::string> &elems) {
	std::stringstream ss(s);
	std::string item;
	while (std::getline(ss, item, delim)) {
		elems.push_back(item);
	}
	return elems;
}


std::vector<std::string> split(const std::string &s, char delim) {
	std::vector<std::string> elems;
	split(s, delim, elems);
	return elems;
}

HANDLE GetProcessByName(TCHAR name[])
{
	HWND hWnd;
	DWORD dwProcessId;
	HANDLE hProcess;
	char szBuildVersionEx[0x1A];

	hWnd = FindWindow(NULL, name);

	GetWindowThreadProcessId(hWnd, &dwProcessId);

	hProcess = OpenProcess((PROCESS_ALL_ACCESS), FALSE, dwProcessId);

	return hProcess;
}

void ReadProcessMemoryRange(HANDLE proc, char* bufferout, INT_PTR readAt, SIZE_T length, SIZE_T& bytesRead)
{
	// Read range
	// If fail return range in half
	// If success append to buffer
	int curReadAt = readAt;
	int curReadLength = length;
	int writeAt = 0;

	while (true)
	{
		if (ReadProcessMemory(proc, (LPCVOID)curReadAt, &bufferout[writeAt], curReadLength, &bytesRead) != 0)
		{
			curReadAt += curReadLength;
			curReadLength = (readAt + length) - curReadAt;
			writeAt += curReadLength;

			// We read everything so we're done
			if (curReadAt == (length + readAt))
			{
				break;
			}
		}
		else
		{
			// Try to read less, if we can't read 1 then return
			if (curReadLength <= 1)
			{
				break;
			}
			curReadLength /= 2;
		}
	}

	SIZE_T totalRead = (curReadAt - readAt);
}

std::vector<Card> HearthstoneMemoryReader::GetCards()
{
	HANDLE proc = GetProcessByName(L"Hearthstone");

	SIZE_T bytesRead = 0;
	INT_PTR readIndex = 0;// 0x18E937EE;

	INT_PTR strAt = 0;

	INT_PTR lastUpdate = readIndex;

	std::vector<INT_PTR> addresses = std::vector<INT_PTR>();
	while (true)
	{
		MEMORY_BASIC_INFORMATION info;
		if (VirtualQueryEx(proc, (LPCVOID)readIndex, &info, sizeof(info)) == 0)
		{
			DWORD error = GetLastError();
			break;
		}

		readIndex = (int)info.BaseAddress;

		if (!((info.State == MEM_COMMIT) && (info.Protect == PAGE_READWRITE) && (info.Type == MEM_PRIVATE)))
		{
			readIndex += info.RegionSize;
			continue;
		}

		INT_PTR startReadAt = readIndex;
		SIZE_T bytesToRead = info.RegionSize;
		char* buffer = new char[bytesToRead];

		ReadProcessMemoryRange(proc, buffer, readIndex, bytesToRead, bytesRead);

		int i = 0;
		for (; i < bytesRead; ++i)
		{
			if (strncmp(&buffer[i], "zonePos=", 8) == 0)
			{
				//std::cout << "Found it" << std::endl;
				strAt = readIndex + i;
				addresses.push_back(strAt);
			}
		}
		readIndex += i + 1;

		delete[] buffer;
	}

	std::vector<Card> cards = std::vector<Card>();
	for (int i = 0; i < addresses.size(); ++i)
	{
		char* buffer = new char[256];

		// Look left until we see '#'
		for (int j = addresses[i]; j > addresses[i] - 200; --j)
		{
			ReadProcessMemory(proc, (LPCVOID)j, buffer, 9, &bytesRead);

			if (buffer[0] == '#')
			{
				addresses[i] = j + 2;
				break;
			}
		}

		ReadProcessMemory(proc, (LPCVOID)addresses[i], buffer, 255, &bytesRead);
		buffer[255] = '\0';

		std::string cardStr(buffer);

		int idx = cardStr.find_first_of('[') - 1;
		int idx2 = cardStr.find_first_of(']');
		std::string cardName = cardStr.substr(0, idx);

		cardStr = cardStr.substr(idx + 2, idx2 - idx - 2);

		std::vector<std::string> subItems = split(cardStr, ' ');

		Card card;
		card.name = cardName;
		for (int j = 0; j < subItems.size(); ++j)
		{
			std::vector<std::string> valuePair = split(subItems[j], '=');

			if (valuePair.size() != 2)
			{
				break;
			}

			if (valuePair[0] == std::string("id"))
			{
				int val = atoi(valuePair[1].c_str());
				card.id = val;
			}
			else if (valuePair[0] == std::string("cardId"))
			{
				card.cardId = valuePair[1];
			}
			else if (valuePair[0] == std::string("zone"))
			{
				card.zone = valuePair[1];
			}
			else if (valuePair[0] == std::string("zonePos"))
			{
				int val = atoi(valuePair[1].c_str());
				card.zonePos = val;
			}
		}
		cards.push_back(card);

		delete[] buffer;
	}

	return cards;
}