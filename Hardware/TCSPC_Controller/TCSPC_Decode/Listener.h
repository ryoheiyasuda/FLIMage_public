#pragma once

class IListener
{
public:
	virtual void HandleNotification(string message) = 0;
};

class Notifier
{
protected:
	virtual void Notify(string message)
	{
		std::for_each(m_listeners.begin(), m_listeners.end(), [&](IListener *l) {l->HandleNotification(message); });
	}

public:
	virtual void RegisterListener(IListener *l)
	{
		m_listeners.insert(l);
	}

	virtual void UnregisterListener(IListener *l)
	{
		std::set<IListener *>::const_iterator iter = m_listeners.find(l);
		if (iter != m_listeners.end())
		{
			m_listeners.erase(iter);
		}
		else 
		{
			// handle the case
			std::cout << "Could not unregister the specified listener object as it is not registered." << std::endl;
		}
	}

private:
	std::set<IListener*> m_listeners;
};

typedef void(__stdcall *callback)(int id, char* message);
class DLL_Listener : public IListener
{
public:
	static DLL_Listener** dllA;


	int id;
	callback myFunc;

	DLL_Listener(int ID, callback myfunc) : num(0)
	{
		id = ID;
		dllA[id] = this;
		myFunc = myfunc;
	}
private:
	int num;

private:
	virtual void HandleNotification(string message)
	{
		char* messageC = new char[message.size() + 1];

		message.copy(messageC, message.size() + 1);
		messageC[message.size()] = '\0';
		myFunc(id, messageC);

		delete messageC;
	}
};
