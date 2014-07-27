// CardWrapper.h

#pragma once

#include <Card.h>
#include <msclr/marshal_cppstd.h>

using namespace System;

namespace HearthstoneMemorySearchCLR
{
	public ref class CardWrapper
	{
		Card* card;

	public:
		CardWrapper()
		{
			this->card = new Card();
		}

		~CardWrapper()
		{
			delete this->card;
		}

		void Init(Card& c)
		{
			this->card = new Card();

			this->Name = gcnew String(c.name.c_str());
			this->CardId = gcnew String(c.cardId.c_str());
			this->Zone = gcnew String(c.zone.c_str());
			this->Id = c.id;
			this->ZonePos = c.zonePos;
		}

		property String ^Name
		{
			String ^get()
			{
				return gcnew String(this->card->name.c_str());
			}

			void set(String ^value)
			{
				this->card->name = std::string(msclr::interop::marshal_as< std::string >(value));
			}
		}

		property String ^CardId
		{
			String ^get()
			{
				return gcnew String(this->card->cardId.c_str());
			}

			void set(String ^value)
			{
				this->card->cardId = std::string(msclr::interop::marshal_as< std::string >(value));
			}
		}

		property String ^Zone
		{
			String ^get()
			{
				return gcnew String(this->card->zone.c_str());
			}

			void set(String ^value)
			{
				this->card->zone = std::string(msclr::interop::marshal_as< std::string >(value));
			}
		}

		property int Id
		{
			int get()
			{
				return this->card->id;
			}

			void set(int value)
			{
				this->card->id = value;
			}
		}

		property int ZonePos
		{
			int get()
			{
				return this->card->zonePos;
			}

			void set(int value)
			{
				this->card->zonePos = value;
			}
		}
	};
}
