namespace GameOffsets
{
    public struct StaticOffsetsPatterns
    {
        public static readonly Pattern[] Patterns =
        {
            // <HowToFindIt>
            // 1: Open CheatEngine and Attach to POE Game
            // 2: Search for String: "InGameState", type: UTF-16 (use case insensitive if you don't find anything in the first try)
            // 3: On all of them, "Find out what accesses this address"
            // 4: Highlight one of the instruction
            // 5: Go through all the registers (e.g. RAX, RBX, RCX) and one of them would be the HashNode Address which points to the InGameState Address.
            // 5.1: To know that this is a HashNode Address make sure that Address + 0x20 points to the "InGameState" key (string)
            // 6: Open HashNode Address in the "Dissect data/structure" window of CheatEngine program.
            // 7: @ 0x08 is the Root HashNode. Copy that value (copy xxxx in i.e. p-> xxxxxx)
            // 7.1: To validate that it's a real Root, 0x019 (byte) would be 1.
            // 8: Pointer scan the value ( which is an address ) you got from step-7 with following configs
            //     Maximum Offset Value: 1024 is enough
            //     Maximum Level: 2 is enough, if you don't find anything increase to 3.
            //     "Allow stack addresses of the first threads": false/unchecked
            //     Rest: doesn't matter
            // 9: Copy the base address and put it in your "Add address manually" button this is your InGameState Address.
            // 10: Do "Find out what accesses this address" and make a pattern out of that function. (pick the one which has smallest offset)
            // </HowToFindIt>
            new(
                "Game States",
                "48 83 EC ?? 48 8B F1 33 ED 48 39 2D ^ ?? ?? ?? ??"
            ),

            // <HowToFindIt>
            // NOTE: This is not a good one, it might break one day.
            // 1: Open CheatEngine and Attach to POE Game
            // 2: Search for String: "Mods.dat", type: UTF-16 (case sensitive is fine), Writable: false/unchecked.
            // 3: "Find out what accesses this address"
            // 4: Highlight one of the instruction (make sure the highlighted instruction is in POE memory, not Kernel/Windows-Lib memory).
            // 5: For each static (green) address on the register RAX, RBX and etc do the following
            // 6: Find the value 1 (or 0) in float format. It must be either before or on that green address. If not, go to step-4 and choose next instruction.
            // 8: Valid address found in step-6, do "Find out what accesses this address" on it.
            // 9: On this instruction do "Find out what addresses this instruction accesses"
            // 10: Go to the game, change area/zone 2 or 3 times.
            // 11: Go back to CheatEngine and sort all the addresses you got and pick the one which is lowest address.
            // 12: On the step-9 instruction do "Break and trace instruction"
            // 12.1: Put (RSI)==0xAddress-of-step8 in the start-condition.
            // 13 From here, find the function which loads that address.
            // </HowToFindIt>
            new(
                "File Root",
                "48 ?? ?? ^ ?? ?? ?? ?? 41 ?? ?? ?? 39 ?? ?? ?? ?? ?? 0F 8E"
            ),

            // <HowToFindIt>
            // This one is really simple/old school CE formula.
            // The only purpose of this Counter is to figure out the files-loaded-in-current-area.
            // 1: Open CheatEngine and Attach to POE Game
            // 2: Search for 4 bytes, Search type should be "UnknownInitialValue"
            // 3: Now Change the Area again and again and on every area change do a "Next Scan" with "Increased Value"
            // 4: U will find 2 green addresses at the end of that search list.
            // 5: Pick the smaller one and create pattern for it.
            // 5.1: Normally pattern are created by "What accesses this address/value"
            // 5.2: but in this case the pattern at "What writes to this address/value" is more unique.
            //
            // NOTE: Reason you picked the smaller one in step-5 is because
            //       the bigger one is some other number which increments by 3
            //       every time you move from Charactor Select screen to In Game screen.
            //       Feel free to test this, just to be sure that the Address you are picking
            //       is the correct one.
            // </HowToFindIt>
            new(
                "AreaChangeCounter",
                "FF ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? FF 05 ^ ?? ?? ?? ?? ?? 8D ?? ?? ?? 8B ??"
            ),

            // <HowToFindIt>
            // Find UiRoot Element
            // Find UiRoot Element Width/Height
            // Do "Who access this Width/Height value"
            // Trace and Break on that instruction with 1000 in Maximum trace count
            // go into all "Call POE.exe +XYZASDAS" functions
            // in one of them, it will be reading floats from static address.
            // ///// Alternative Approach
            // Change game window Height and Width (when in the login screen)
            // Look for float which increases when Width increases and decreases when width decreases max = 1.0 min = 0.3
            // Now look for who access this address
            // </HowToFindIt>
            // this is the pattern.
            new(
                "GameWindowScaleValues",
                "C7 ?? 00 00 80 3F C7 ?? 04 00 00 80 3F C3 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? C3 ?? ?? ?? ?? ^ ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? C3 ?? ?? ?? ??"
            ),

            // <HowToFindIt>
            // Find player -> Render component -> TerrainHeight.
            // Do "What writes to this address" on terrainheight.
            // This instruction which writes to terrainheight, also writes to 200 different address
            // So let's narrow down the invalid result by putting the following Start Condition to it.
            //      if instruction in step-2 is as following
            //      mov [RAX+C8], xmmm0;
            //      then Start Condition will be (RAX==0xRenderCompAddress+(TerrainheightOffset - C8))
            //      CE can't do +, - on Start condition so calculate it via a calculator. Final condition e.g. (RAX==0x123123123)
            // Go to the top of the function you found in step - 2 (u can right click on the statement and select "select current func" and repeat the last step with exact same condition.
            // In your "Trace and break" window that u got from the last step, 3rd or 4th function from the top will be the function from which this pattern is created.
            //          that function will be the first function in that whole window that has more than 10 instructions, every function before this function will have
            //          2 or 3 or 4 instructions max.
            // </HowToFindIt>
            new(
                "Terrain Rotator Helper",
                "4c ?? ?? ?? 48 ?? ?? ^ ?? ?? ?? ?? 4c ?? ?? 8b ?? 2b ?? 33 ??"
            ),

            // Same as above pattern, just added 23 ?? because the data is actually before the pattern.
            new(
                "Terrain Rotation Selector",
                "^ ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 4c ?? ?? ?? 48 ?? ?? ?? ?? ?? ?? 4c ?? ?? 8b ?? 2b ?? 33 ??"
            )
        };
    }
}