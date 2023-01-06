
/*
██████╗  █████╗ ████████╗████████╗██╗     ███████╗    ██████╗  ██████╗  █████╗ ████████╗
██╔══██╗██╔══██╗╚══██╔══╝╚══██╔══╝██║     ██╔════╝    ██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
██████╔╝███████║   ██║      ██║   ██║     █████╗      ██████╔╝██║   ██║███████║   ██║   
██╔══██╗██╔══██║   ██║      ██║   ██║     ██╔══╝      ██╔══██╗██║   ██║██╔══██║   ██║   
██████╔╝██║  ██║   ██║      ██║   ███████╗███████╗    ██████╔╝╚██████╔╝██║  ██║   ██║   
╚═════╝ ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚══════╝╚══════╝    ╚═════╝  ╚═════╝ ╚═╝  ╚═╝   ╚═╝   
  _  ___  _     ____  ____  _  _  __    __   
 / )/ __)( \   (  _ \(_  _)( \/ )/__\  (  )  
( (( (__  ) )   )   / _)(_  \  //(__)\  )(__ 
 \_)\___)(_/   (_)\_)(____)  \/(__)(__)(____)
*/

using System.Text;

//Map constants
const String Water = "~";
const String Ship = "=";
const String Miss = "o";
const String Hit = "*";
const String VBow = "V";
const String VStern = "╓";
const String VShip = "║";
const String HBow = ">";
const String HShip = "==";
const String HStern = "╘=";

//Board constants
const String Easting = "ABCDEFGH";
const String Northing = "12345678";
const String Vertical = "V";
const String Horizontal = "H";
const String Orientations = "VH";
const int Height = 8;
const int Width = 8;

//Menu constants
const String MenuOptions = "SLQD";

//Ship constants
const int Destroyer = 3;
const int Submarine = 2;
const int Frigate = 1;
const int MaxShips = 5;

//Player constants
const int Player = 1;
const int Computer = 2;
const int NoWinner = 0;

//Text constants
const String MissText = "Miss!";
const String HitText = "*** ITS A HIT ***";
const String SunkText = "Ship has been sunk!";
const String ComputerAttacksText = "Computer Attacks {0}{1}!";
const String PrepareForBattleText = "Prepare for Battle!";
const String EastingText = "Enter your easting";
const String NorthingText = "Enter your northing";
const String NextShipText = "Place your next ship";
const String InvalidLocationText = "Invalid location";
const String OrientationText = "Enter orientation";
const String ClearText = "                                        ";
const String PlayerWinsText = "Congratulations, you are the winner";
const String ComputerWinsText = "What!  You let the random computer win?";
const String SaveGameFilename = "SaveGame.txt";
const String MenuTitleText = "Select an option from the menu";
const String NewGameText = "S - start a new game";
const String LoadGameText = "L - load game";
const String QuitText = "Q - quit";
const String DebugModeText = "Debug mode enabled";

//Save game constants
const String shipSplitter = "+";
const String boardSplitter = "[";
const String dataSplitter = ";";

//Global Variables
String[] PlayerAttackBoard = new String[Height * Width];
String[] PlayerDefenceBoard = new String[Height * Width];
String[] ComputerAttackBoard = new String[Height * Width];
String[] ComputerDefenceBoard = new String[Height * Width];

//Store the locations of each of the players ships, so we can check the board
//to see when they are destroyed.
String[] PlayerShips = new String[MaxShips];
String[] ComputerShips = new String[MaxShips];

int[] AvailableShips = new int[MaxShips] {Destroyer, Submarine, Submarine, Frigate, Frigate};
String[] ShipNames = new String[] {"Destroyer", "Submarine", "Submarine", "Frigate", "Frigate"};

//Player goes first
int CurrentPlayer = Player;
int Winner = NoWinner;

//Debug mode.  If mode is true, show the defender's defence board as well
bool Debug = false;

//First game - User places their ships
void placePlayerShips()
{
    //Clear the play area
    clear(8);
    int placedShips = 0;
    while(placedShips < MaxShips)
    {
        //Draw the board
        drawBoard(44, 7, PlayerDefenceBoard);
        if(Debug) drawBoard(62, 7, ComputerDefenceBoard);

        //Ask the player to place a ship
        message(NextShipText);
        writeAt(5, 9, ShipNames[placedShips]);

        //Read in coordinates
        String coord = "";
        String orientation = Vertical; //Default orientation
        
        //If the length of the ship is > 1 then get an orientation 
        if(AvailableShips[placedShips] > 1 )
        {
            coord = getCoordWithOrientation();
            orientation = coord.Substring(2, 1);
        }
        else //coordinates are fine
        {
            coord = getCoord();
        }

        //Can we place a ship here?
        if(isValidLocation(PlayerDefenceBoard, coord, AvailableShips[placedShips]))
        {
            //Add it to the board
            int pos = coordToPosition(coord);
            placeShip(PlayerDefenceBoard, pos, AvailableShips[placedShips], orientation);

            //Add the new ship to the PlayerShips array
            addToPlayerShips(placedShips, pos, AvailableShips[placedShips], orientation);

            //move onto the next ship
            placedShips++;
        }
    }
}

//Second game - computer places its ships
void placeComputerShips()
{
    //We should just find 5 random valid places for the computer
    int placedShips = 0;
    Random random = new Random();
    while(placedShips < MaxShips)
    {
        //Choose a random number between 0 and 63
        int pos = random.Next(Height * Width - 1);

        //Choose an orientations
        String orientation = Vertical; //default
        if(AvailableShips[placedShips] > 1) //Check the length of the ship
        {
            //This needs an orientation
            if(random.Next() % 2 == 0)
            {
                //On an even number choose horizontal
                orientation = Horizontal;
            }
        }

        if(isValidPosition(ComputerDefenceBoard, pos, AvailableShips[placedShips], orientation))
        {
            //okay - we can use this position
            placeShip(ComputerDefenceBoard, pos, AvailableShips[placedShips], orientation);

            //Add the new ship to the ComputerShips array
            addToComputerShips(placedShips, pos, AvailableShips[placedShips], orientation);

            //move onto the next ship
            placedShips++;
        }
    }
}

//Third game - a battle to the death
void startBattle()
{
    //Clear the playing area
    clear(6);
    message(PrepareForBattleText);

    //This is the main loop for actually playing the game
    while(Winner == NoWinner)
    {
        move(CurrentPlayer);

        //pause to allow the user to see what happened
        Thread.Sleep(2000);// 2 seconds
    }

    //Winner!
    clear(6);
    if(Winner == Player)
    {
        centre(12, PlayerWinsText);
    }
    else //Computer
    {
        centre(12, ComputerWinsText);
    }

    //Wait for any button press
    readKey();

    //let this function end and the main loop will restart
}

// The player makes a move
void move(int player)
{
    int move = 0;
    if(player == Player)
    {
        //Draw the player's attacking board
        drawBoard(44, 7, PlayerAttackBoard);
        if(Debug) drawBoard(62, 7, ComputerDefenceBoard);

        //Get the square from the player
        move = getPlayerMove();
        //Check to see if it has hit anything
        resolveMove(ComputerDefenceBoard, PlayerAttackBoard, move);
        //Check all the ships to see if anything is sunk
        checkShips(ComputerDefenceBoard, ComputerShips);
        //Update the board on screen with the new move
        drawBoard(44, 7, PlayerAttackBoard);
        //Clean up screen
        instruction(ClearText);

        //Change player
        CurrentPlayer = Computer;
    }
    else //Computer
    {
        //Draw the computers's attacking board
        drawBoard(44, 7, ComputerAttackBoard);
        if(Debug) drawBoard(62, 7, PlayerDefenceBoard);
        
        //Generate a random move
        move = getComputerMove();
        //Check to see if it has hit anything
        resolveMove(PlayerDefenceBoard, ComputerAttackBoard, move);
        //Check all the ships to see if anything is sunk. 
        //Computer is attacking, so check the player's board and ships
        checkShips(PlayerDefenceBoard, PlayerShips);
        //Update the board on screen with the new move
        drawBoard(44, 7, ComputerAttackBoard);
        //Change player
        CurrentPlayer = Player;

        //Save the game after every computer move
        save();
    }
}

int getPlayerMove()
{
    String coord = getCoord();
    return coordToPosition(coord);
}

//Lots of different strategies possible.  Lets go with random fire right now.
int getComputerMove()
{
    Random random = new Random();
    bool isValid = false;
    int pos = 0;

    while(!isValid)
    {
        pos = random.Next(Height * Width -1);
        //Random, but check that we've not fired here already
        if(ComputerAttackBoard[pos] == Water)
        {
            isValid = true;
        }
    }

    //Make a string to show to the player. We need to convert the integer pos back to a human reading coordinate.
    String mess = String.Format(ComputerAttacksText, Easting.ToCharArray()[pos % Width], Northing.ToCharArray()[pos / Width]);
    instruction(mess);

    return pos;
}

//Fire upon board at pos
void resolveMove(String[] defence, String[] attack, int pos)
{
    //Lets see what we hit
    String target = defence[pos];

    if(target == Water)
    {
        //Its a straight miss
        defence[pos] = Miss;
        attack[pos] = Miss;
        message(MissText);
    }
    else if(target == Miss || target == Hit)
    {
        //You've already fired here - do nothing!
    }
    else //deduction tells us that this must be a hit!
    {
        Console.Beep();
        defence[pos] = Hit;
        attack[pos] = Hit;
        message(HitText);
    }
}

//Check each ship agains the board and see if any are newly sunk
void checkShips(String[] board, String[] ships)
{
    int sunk = 0;
    for(int i =0; i<MaxShips; i++)
    {
        if(checkShip(board, ships[i]))
        {
            sunk++; //:-(

            //we need to save this back to the list of ships
            String ship = ships[i];
            ship = ship.Replace('n', 'y');
            ships[i] = ship;
        }
    }

    if(sunk == MaxShips)
    {
        //The end of the game!
        Winner = CurrentPlayer;
    }
}

//Check a specfic ship to see if it is newly sunk
//return true if the ship is sunk
bool checkShip(String[] board, String ship)
{
    bool isSunk = false;
    String[] details = ship.Split('|');
    int pos = Convert.ToInt32(details[0]);
    int length = Convert.ToInt32(details[1]);
    String orientation = details[2];
    String sunk = details[3];
    if(sunk == "y")
    {
        //This ship is already sunk
        isSunk = true;
    }
    else
    {
        //We need to know if we are moving vertically or horzontally
        int inc = 1;
        if(orientation == Vertical)
        {
            inc = Width;
        }
        int hits = 0;
        //We need to check every square
        for(int i=0; i<length;i++)
        {
            //check this square for a hit
            if(board[pos] == Hit)
            {
                //Oops
                hits++;
            }

            //Move to next square
            pos += inc;
        }

        //If hits and length are the same, then this boat is sunk
        if(hits == length)
        {
            //You've sunk my Battleboat!
            isSunk = true;
            message(SunkText);
        }
    }

    return isSunk;
}

void addToPlayerShips(int index, int pos, int length, String orientation)
{
    addToShips(index, PlayerShips, pos, length, orientation);
}

void addToComputerShips(int index, int pos, int length, String orientation)
{
    addToShips(index, ComputerShips, pos, length, orientation);
}

//Adds a ship to the players collection.  We can use this to check for sunk ships
//later in the game.
void addToShips(int index, String[] ships, int pos, int length, String orientation)
{
    //String format is
    //<position>|<length>|<orientation>|<y/n if the ship is sunk>
    //Ships are never sunk when they are added to this array
    String ship = String.Format("{0}|{1}|{2}|n", pos, length, orientation);

    ships[index] = ship;
}

void placeShip(String[] board, int pos, int length, String orientation)
{
    int inc = 0;
    if(orientation == Horizontal)
    {
        inc = 1;
    }
    else //vertical
    {
        inc = 8;
    }

    for(int i =0; i< length; i++)
    {
        board[pos] = getShipChar(i, length, orientation);
        pos += inc;
    }
}

String getShipChar(int pos, int length, String orientation)
{
    if(orientation == Horizontal)
    {
        return getHorizontalShipChar(pos, length);
    }
    else //vertical
    {
        return getVerticalShipChar(pos, length);
    }
}

String getHorizontalShipChar(int pos, int length)
{
    String ret = "";

    if(length == 1)
    {
        ret = Ship;
    }
    else if(pos == 0)
    {
        ret = HStern;
    }
    else if(pos == length-1)
    {
        ret = HBow;
    }
    else
    {
        ret = HShip;
    }

    return ret;
}

String getVerticalShipChar(int pos, int length)
{
    String ret = "";

    if(length == 1)
    {
        ret = Ship;
    }
    else if(pos == 0)
    {
        ret = VStern;
    }
    else if(pos == length -1)
    {
        ret = VBow;
    }
    else
    {
        ret = VShip;
    }

    return ret;
}

// Turn a human-readable coordinate (E7) to an integer index position
int coordToPosition(String coord)
{
    //We need to turn the easting and northing into integers
    String easting = coord.Substring(0, 1);
    //Use the position of the letter in the string to determine column (zero based)
    int col = Easting.IndexOf(easting);

    //Now convert the northing to an int, and subtract 1 to make it zero based as well
    String northing = coord.Substring(1, 1);
    int row = Convert.ToInt32(northing);
    row = row - 1;

    return (row * 8) + col;
}

//Returns true if there is space for the length of the ship, at the position indicated, on the board provided.
bool isValidLocation(String[] board, String coord, int length)
{
    //Lets assume that the basic coordiate is correct
    int pos = coordToPosition(coord);

    //If we have 3 characters, the last one is an orientation
    String orientation  = "";
    if(coord.Length == 3)
    {
        orientation = coord.Substring(2, 1);
    }

    return isValidPosition(board, pos, length, orientation);
}

//Same as isValidLocation, but works with an index number instead of a human-readable coordinate
bool isValidPosition(String[] board, int pos, int length, String orientation)
{
    //We're going to assume that it is valid until we find an invalid square
    bool isValid = true;

    //We have a start location and an orientation
    //Validate every position
    int inc = 0;
    if(orientation == Horizontal)
    {
        inc = 1;
    }
    else //Vertical
    {
        inc = Width;
    }

    //If the orientation is horizontal, check that there is enough width on 
    //the row to contain the whole ship.  We don't need to do this vertically,
    //as the pos > 64 if it goes off the edge.  Width is defined as a constant.
    if(orientation == Horizontal)
    {
        if((pos % Width) + length > Width)
        {
            //Hanging off the edge and will wrap around onto the next row
            isValid = false;
        }
    }

    //Check every piece of the ship, if we are still valid
    if(isValid)
    {
        for(int i = 0; i < length; i++)
        {
            //Check the boundaries of row and col
            if(pos < 0 || pos > Width * Height)
            {
                isValid = false;
                break;
            }

            //Check location is open water
            if(board[pos] != Water)
            {
                isValid = false;
                break;
            }

            //Increment - 1 for horizontal and 8 for vertical
            pos += inc;
        }
    }

    //Report the error
    if(!isValid)
    {
        message(InvalidLocationText);
    }

    return isValid;
}

//Save the current game state to 
//Format is that all 4 boards are interlaces in this order:
//PlayerAttackBoard
//PlayerDefenceBoard
//ComputerAttackBoard
//ComputerDefenceBoard
//then the ships array for Player and then Computer
//We are going to rely heavily on some special characters to use
//as limiters between the data elements
void save()
{
    //Use a StringBuilder to create the save game
    StringBuilder saveGame = new StringBuilder();
    for(int i=0; i<Height*Width; i++)
    {
        saveGame.Append(PlayerAttackBoard[i]);
        saveGame.Append(boardSplitter);
        saveGame.Append(PlayerDefenceBoard[i]);
        saveGame.Append(boardSplitter);
        saveGame.Append(ComputerAttackBoard[i]);
        saveGame.Append(boardSplitter);
        saveGame.Append(ComputerDefenceBoard[i]);
        saveGame.Append(boardSplitter);
    }

    //Add a character so we cah split the board positions from the
    //ships data
    saveGame.Append(dataSplitter);

    //Interlace the ships, but separate them with a symbol that doesn't exist
    //in the data we are saving
    for(int i=0; i<MaxShips; i++)
    {
        saveGame.Append(PlayerShips[i]);
        saveGame.Append(shipSplitter); //separate values by a + symbol.
        saveGame.Append(ComputerShips[i]);
        saveGame.Append(shipSplitter); //separate values by a + symbol.
    }

    File.WriteAllText(SaveGameFilename, saveGame.ToString());
}

//Load the game state from disk
void load()
{
    String saveGame = File.ReadAllText(SaveGameFilename);

    //Split into board data and ship data
    String[] allData = saveGame.Split(dataSplitter);
    String boardData = allData[0];
    String shipData = allData[1];

    //Split the board data into squares
    String[] squares = boardData.Split(boardSplitter);
    //Write the squares back into the correct boards
    for(int i =0; i<Height*Width; i++)
    {
        PlayerAttackBoard[i] = squares[i*4];
        PlayerDefenceBoard[i] = squares[i*4+1];
        ComputerAttackBoard[i] = squares[i*4+2];
        ComputerDefenceBoard[i] = squares[i*4+3]; //eg when i = 5 then the correct index will be 5*4 (20) + 3 to make index 23
    }

    //Now read out the ship statuses
    String[] allShips = shipData.Split(shipSplitter); //Split on the + character
    for(int i=0; i<MaxShips; i++)
    {
        PlayerShips[i] = allShips[i*2];
        ComputerShips[i] = allShips[(i*2)+1]; // First iteration read 1, 2nd iteration read 3, 3rd read 5 etc.  Don't forget that i is zero based.
    }
}


// User inputs
//Returns a string with Easting, Northing and Orientation
String getCoordWithOrientation()
{
    //Get the basic coordinates
    String coord = getCoord();

    if(coord.Length == 2)
    {
        coord = coord + getOrientation();
    }

    return coord;
}

//Returns a 2 character string from the user
String getCoord()
{
    String coord = "";

    while(coord.Length < 2)
    {
        switch(coord.Length)
        {
            case 0:
                coord = getEasting();
                break;
            case 1:
                coord = coord + getNorthing();
                break;
        }
    }
    return coord;
}

//Read an Easting (e.g. ABC...H)
String getEasting()
{
    return getInput(EastingText, Easting);
}

//Read an Northing (e.g. 123...8)
String getNorthing()
{
    return getInput(NorthingText, Northing);
}

String getOrientation()
{
    return getInput(OrientationText, Orientations);
}

String getInput(String text, string validKeys)
{
    instruction(text);

    String key = "";
    bool isValid = false;
    while(!isValid)
    {
        key = readKey();
        if(validKeys.Contains(key))
        {
            isValid = true;
        }
    }

    return key;
}

//Read a single keypress from the user
String readKey()
{
    //Read a key press - do not show the key on the console
    ConsoleKeyInfo key = Console.ReadKey(true);
    return key.KeyChar.ToString().ToUpper();
}

//Get a valid menu selection from the user
String getMenuOption()
{
    bool haveResponse = false;
    String response = "";

    while(!haveResponse)
    {
        response = readKey();

        if(MenuOptions.Contains(response))
        {
            haveResponse = true;
        }
    }

    return response;
}

void init()
{
    Console.Clear();
    centre(0, " ____        _   _   _        ____              _   ");
    centre(1, "|  _ \\      | | | | | |      |  _ \\            | |  ");
    centre(2, "| |_) | __ _| |_| |_| | ___  | |_) | ___   __ _| |_ ");
    centre(3, "|  _ < / _` | __| __| |/ _ \\ |  _ < / _ \\ / _` | __|");
    centre(4, "| |_) | (_| | |_| |_| |  __/ | |_) | (_) | (_| | |_ ");
    centre(5, "|____/ \\__,_|\\__|\\__|_|\\___| |____/ \\___/ \\__,_|\\__|");
    Console.Beep();
    if(Debug)
    {
        writeAt(3, 38, DebugModeText);
    }
    else
    { 
        writeAt(3, 38, ClearText);
    }
}

//Set all of the squares in all of the boards to be open water, ready for a new game
void reset()
{
    for(int i = 0; i<Height * Width; i++)
    {
        PlayerAttackBoard[i] = Water;
        PlayerDefenceBoard[i] = Water;
        ComputerAttackBoard[i] = Water;
        ComputerDefenceBoard[i] = Water;
    }

    //Reset player ships
    for(int i = 5; i < MaxShips; i++)
    {
        PlayerShips[i] = "";
        ComputerShips[1] = "";
    }

    CurrentPlayer = Player;
    Winner = NoWinner;
}

void menu()
{
    //Display the options to the user
    centre(8, MenuTitleText);
    centre(10, NewGameText);
    centre(12, LoadGameText);
    centre(14, QuitText);
}


//Draw any board at any position on the console
void drawBoard(int x, int y, String[] board)
{
    //Write the Easting and Northing,  Assumes a square board
    for(int i = 0; i < Width; i++)
    {
        /*Write the letters across the top.  Position is:
         x - from the parameters
         i * 2 - double space the letters
         + 1 - to give room for the northings */
        writeAt(x + (i * 2) + 1, y, Easting.Substring(i, 1));
        writeAt(x, y + i + 1, Northing.Substring(i, 1));
    }

    //Write out the contents of the board
    for(int i=0; i<Height * Width; i++)
    {
        //For each square, write the piece at
        //(x + i) MOD 8 and
        //(y + i) DIV 8
        int xpos = x + (i % Width); //This is an extra padding for double width X positions
        xpos += i % Width; //wrap every 8 characters
        xpos += 1; //Avoid the northings column

        int ypos = y; //Start with the position
        ypos += i / Width; //move down every 8 pieces
        ypos += 1; //Avoid the eastings row
        writeAt(xpos, ypos, board[i]);
    }
}

//Write a string at a given location on the console
void writeAt(int x, int y, String text)
{
    Console.SetCursorPosition(x, y);
    Console.Write(text);
}

//Centre a line in the console window
void centre(int y, String text)
{
    //Subtract the length of the text from the width of the console, and divide by 2
    int x = (Console.BufferWidth - text.Length) / 2;
    writeAt(x, y, text);
}

//Write a message at Row 8
void message(String text)
{
    //Write 40 spaces to blank the area
    writeAt(1,8,"                                        ");
    writeAt(1,8, text);
}

//Write an instruction at Row 10
void instruction(String text)
{
    //Write 40 spaces to blank the area
    writeAt(1,12,"                                        ");
    writeAt(1,12, text);
}

//Clear the console window from the start row
void clear(int startRow)
{
    //Never clear the last row - it might have the debug messge
    for(int row = startRow; row < Console.BufferHeight -1; row++)
    {
        for(int col = 0; col < Console.BufferWidth; col++)
        {
            writeAt(col, row, " ");
        }
    }
}

/******
 * Program execution starts here 
 *******/

bool quit = false;

while(!quit)
{
    init();
    reset();
    menu();
    String option = getMenuOption();        
    switch(option)
    {
        case "S":
            //New 
            placeComputerShips();
            placePlayerShips();
            startBattle();
            break;
        case "L":
            //load game
            load();
            startBattle();
            break;  
        case "D":
            //Hidden debug mode!
            Debug = !Debug;
            break;
        case "Q":
            //Quit
            quit = true;
            break;
    }
}

//End of main program