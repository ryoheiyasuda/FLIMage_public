// ConsoleApplication1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "MemoryManager.h"

int main()
{
	cout << "Press any key to start \n";
	std::getchar();

	int nChannels = 3;
	int* nTime = new int[nChannels];
	for (int i = 0; i < nChannels; i++)
		nTime[i] = 32;
	//nTime[2] = 0;

	std::cout << nTime[0] << "\n" << nTime[1] << "\n";

	//Starting new memory manager.
	int ID = 0;
	MemoryManager::CreateMemoryManager(ID);
	MemoryManager* mm = MemoryManager::memInstances[ID]; //obtain memory instance for this instance.

	if (MemoryManager::instanceStatus[ID] == 1)
		mm->InitializeMemory(nChannels, 32, 128, 128, nTime);

	int* erase = new int[nChannels];
	erase[0] = 1;
	erase[1] = 1;
	erase[2] = 1;

	//Testing AddtoPixels and SwitchMemoryBank.
	for (int i = 0; i < 10; i++)
	{
		for (int j = 0; j < 10 + i; j++)
			mm->AddToPixels(1, 16, 32, 32, 12);

		std::cout << "Done = " << mm->doneBank << ": ";

		std::cout << mm->GetValue(0, 1, 16, 32, 32, 12) << ", ";
		std::cout << mm->GetValue(1, 1, 16, 32, 32, 12) << ", ";
		std::cout << mm->GetValue(2, 1, 16, 32, 32, 12) << ", ";
		std::cout << "\n";

		//cout << "Erase =" << erase[0] << "," << erase[1] << "," << erase[2] << "\n";
		//cout << "Erase pointer =" << erase << "\n";
		mm->SwitchMemoryBank(erase);

	}

	std::cout << "\n";
	mm->InitializeMemory(nChannels, 32, 128, 128, nTime);

	erase[0] = 0;
	erase[1] = 0;
	erase[2] = 0;

	std::cout << nTime[0] << "\n" << nTime[1] << "\n";
	std::cout << "Starting averaing\n";

	for (int i = 0; i < 10; i++)
	{
		for (int j = 0; j < 10 + i; j++)
			mm->AddToPixels(1, 16, 32, 32, 12);


		std::cout << "Done = " << mm->doneBank << ": ";

		mm->SwitchMemoryBank(erase);

		std::cout << mm->GetValue(0, 1, 16, 32, 32, 12) << ", ";
		std::cout << mm->GetValue(1, 1, 16, 32, 32, 12) << ", ";
		std::cout << mm->GetValue(2, 1, 16, 32, 32, 12) << ", ";
		std::cout << "\n";

	}

	delete[] erase;
	delete[] nTime;
	mm->deleteMemoryBank();

	getchar();
}

