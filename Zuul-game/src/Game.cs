using System;
using System.Reflection.Metadata;
//Using system Tyrese
class Game
{
	// Private 
	private Parser parser;
	private Player player;
	private bool keyUsed = false;
	Item key = new Item(15, "goldbar");

	Item medkit = new Item(20, "apple");
	// De Constructor
	public Game()
	{
		parser = new Parser();
		player = new Player();
		CreateRooms();
	}

	// Create the rooms
	private void CreateRooms()
	{
		// Create the rooms
		Room outside = new Room("outside the main entrance of an abandoned mansion");
		Room theatre = new Room("in the mansion's theatre");
		Room pub = new Room("in the mansion's pub");
		Room lab = new Room("in the mansion's experiment lab");
		Room office = new Room("in the mansion's admin office");
		Room hallway = new Room("in the mansion's hallway");
		Room up = new Room("on the second floor of the mansion");
		Room secretvault = new Room("in the secret vault");
		// Initialise room exits
		outside.AddExit("east", theatre);
		outside.AddExit("south", lab);
		outside.AddExit("west", pub);

		theatre.AddExit("west", outside);
		theatre.AddExit("door", hallway);

		hallway.AddExit("theatre", theatre);
		hallway.AddExit("stairs", up);

		up.AddExit("down", hallway);
		up.AddExit("secret_vault", secretvault);

		secretvault.AddExit("hallway", hallway);

		pub.AddExit("east", outside);

		lab.AddExit("north", outside);
		lab.AddExit("east", office);

		office.AddExit("west", lab);

		secretvault.Chest.Put("goldbar", key);
		theatre.Chest.Put("apple", medkit);
		// Start game outside
		player.CurrentRoom = outside;
	}

	//  Main play that loops until end.
	public void Play()
	{
		PrintWelcome();

		// Enter the main command loop. Here we repeatedly read commands and
		// execute them until the player wants to quit.
		bool finished = false;
		while (!finished)
		{
			Command command = parser.GetCommand();
			finished = ProcessCommand(command);
		}
		Console.WriteLine("Thank you for playing.");
		Console.WriteLine("Press [Enter] to continue.");
		Console.ReadLine();
	}

	// Print out the opening message for the player.
	private void PrintWelcome()
	{
		Console.WriteLine();
		Console.WriteLine("Welcome to Zuul!");
		Console.WriteLine("Zuul is a new, incredibly boring adventure game.");
		Console.WriteLine("Type 'help' if you need help.");
		Console.WriteLine();
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));
	}

	// Given a command, process (that is: execute) the command.
	// If this command ends the game, it returns true.
	// Otherwise false is returned.
private bool ProcessCommand(Command command)
{
	bool wantToQuit = false;

	if (!player.IsAlive() && command.CommandWord != "quit")
	{
		Console.WriteLine("You bled out, and are now dying...");
		Console.WriteLine("You may only use the command:");
		Console.WriteLine("quit");
		return wantToQuit;
	}

	if (keyUsed && command.CommandWord != "quit") 
	{
		Console.WriteLine("You have won the game!, the only allowed command is 'quit'.");
		return wantToQuit;
	}

	if (command.IsUnknown())
	{
		Console.WriteLine("I do not understand what that means...");
		return wantToQuit;
	}

    switch (command.CommandWord)
    {
        case "help":
            PrintHelp();
            break;
        case "check":
            Look();
            break;
		case "take":
			Take(command);
			break;
		case "drop":
			Drop(command);
			break;
        case "status":
            Health();
            break;
        case "go":
            GoRoom(command);
            break;
		case "use":
			UseItem(command, out keyUsed); 
			break;

		case "quit":
			wantToQuit = true;
			break;
	}

	return wantToQuit;
}

	// ######################################
	// implementations of user commands:
	// ######################################
	
	// Print out some help information.
	// Here we print the mission and a list of the command words.
	private void PrintHelp()
	{
		Console.WriteLine("You are lost. You are alone.");
		Console.WriteLine("You wander around at the abandoned mansion.");
		Console.WriteLine();
		// let the parser print the commands
		parser.PrintValidCommands();
	}

	private void Look()
	{
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));

		Dictionary<string, Item> roomItems = player.CurrentRoom.Chest.GetItems();
		if (roomItems.Count > 0)
		{
			Console.WriteLine("Current items in this room:");
			foreach (var itemEntry in roomItems)
			{
				Console.WriteLine($"{itemEntry.Value.Description} - ({itemEntry.Value.Weight} kg)");
			}
		}
	}


	private void Take(Command command)
	{
		if (!command.HasSecondWord())
		{
			Console.WriteLine("Take what?");
			return;
		}

		string itemName = command.SecondWord.ToLower();

		bool success = player.TakeFromChest(itemName);

	}

	private void Drop(Command command)
	{
		if (!command.HasSecondWord())
		{
			Console.WriteLine("drop what?");
			return;
		}

		string itemName = command.SecondWord.ToLower();

		bool success = player.DropToChest(itemName);


	}

	private void Health()
	{
		Console.WriteLine($"your current health is: {player.GetHealth()}");

		Dictionary<string, Item> items = player.GetItems();

		if (items.Count > 0)
		{
			Console.WriteLine("your current items are:");

			// Iterate over elk item in player zijn inv
			foreach (var itemEntry in items)
			{
				Console.WriteLine($"- {itemEntry.Key}: weight {itemEntry.Value.Weight}");
			}
		}
		else
		{
			Console.WriteLine("you currently have no items in your inventory.");
		}
	}

	
	// Try to go to one direction. If there is an exit, enter the new
	// room, otherwise print an error message.
	private void GoRoom(Command command)
	{
		if(!command.HasSecondWord())
		{
			// if there is no second word, we don't know where to go...
			Console.WriteLine("Go where?");
			return;
		}

		string direction = command.SecondWord;

		// Try to go to the next room.
		Room nextRoom = player.CurrentRoom.GetExit(direction);
		if (nextRoom == null)
		{
			Console.WriteLine("There is no door to "+direction+"!");
			return;
		}

		player.Damage(15);
		player.CurrentRoom = nextRoom;
		Console.WriteLine(player.CurrentRoom.GetLongDescription(player));
		if (player.CurrentRoom.GetExit("door") != null)
		{
			Console.WriteLine("You found a door in the theatre, it seems like it leads to the mansion's hallway.");
		}
		
		if (!player.IsAlive()) 
		{
			Console.WriteLine("Your vision blurs, the world seems to be spinning. Your wounds draining your strength. You collapse, you are now bleeding out..");
		}
	}

    private void UseItem(Command command, out bool keyUsed)
    {
        if (!command.HasSecondWord())
        {
            Console.WriteLine("Use what?");
            keyUsed = false;
            return;
        }

        string itemName = command.SecondWord.ToLower();

        bool itemUsed = player.Use(itemName, out keyUsed);

        if (itemUsed)
        {
            if (keyUsed)
            {
                this.keyUsed = true; 
                Console.WriteLine("You've called 911, and after calling and ambulance and police car are on their way...");
				Console.WriteLine("Your vision blurs as you lose consciousness...");
				Console.WriteLine("You regain consciousness later on, you are in an ambulance...");
				Console.WriteLine(" ");
				Console.WriteLine("Congratulations!, you have beaten the game.");
            }
        }
    }
}




