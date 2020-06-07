namespace GameOffsets
{
    public struct StaticOffsetsPatterns
    {
        public static Pattern[] patterns =
        {
            ///<HowToFindIt>
            /// 1: Open CheatEngine and Attach to POE Game
            /// 2: Search for String: "InGameState", type: UTF-16 (use case insensitive if you don't find anything in the first try)
            /// 3: On all of them, "Find out what accesses this address"
            /// 4: Highlight one of the instruction
            /// 5: Go through all the registers (e.g. RAX, RBX, RCX) and one of them would be the HashNode Address which points to the InGameState Address.
            /// 5.1: To know that this is a HashNode Address make sure that Address + 0x20 points to the "InGameState" key (string)
            /// 6: Open HashNode Address in the "Dissect data/structure" window of CheatEngine program.
            /// 7: @ 0x08 is the Root HashNode. Copy that value (copy xxxx in i.e. p-> xxxxxx)
            /// 7.1: To validate that it's a real Root, 0x019 (byte) would be 1.
            /// 8: Pointer scan the value ( which is an address ) you got from step-7 with following configs
            ///     Maximum Offset Value: 1024 is enough
            ///     Maximum Level: 2 is enough, if you don't find anything increase to 3.
            ///     "Allow stack addresses of the first threads": false/unchecked
            ///     Rest: doesn't matter
            /// 9: Copy the base address and put it in your "Add address manually" button this is your InGameState Address.
            /// 10: Do "Find out what accesses this address" and make a pattern out of that function. (pick the one which has smallest offset)
            /// 
            /// ==Function start==to==10 lines after== 
            /// PathOfExile_x64.exe+149650 - 40 57                 - push rdi
            /// PathOfExile_x64.exe+149652 - 48 83 EC 40           - sub rsp,40 { 64 }
            /// PathOfExile_x64.exe+149656 - 48 C7 44 24 20 FEFFFFFF - mov qword ptr [rsp+20],FFFFFFFFFFFFFFFE { -2 }
            /// PathOfExile_x64.exe+14965F - 48 89 5C 24 58        - mov [rsp+58],rbx
            /// PathOfExile_x64.exe+149664 - 48 89 6C 24 60        - mov [rsp+60],rbp
            /// PathOfExile_x64.exe+149669 - 48 89 74 24 68        - mov [rsp+68],rsi
            /// PathOfExile_x64.exe+14966E - 48 8B F9              - mov rdi,rcx
            /// PathOfExile_x64.exe+149671 - 33 ED                 - xor ebp,ebp
            /// PathOfExile_x64.exe+149673 - 48 39 2D C680F001     - cmp [PathOfExile_x64.exe+2051740],rbp { (1F8B94DED50) } <------- Game State Address
            /// PathOfExile_x64.exe+14967A - 0F85 5F010000         - jne PathOfExile_x64.exe+1497DF
            /// PathOfExile_x64.exe+149680 - 65 48 8B 04 25 58000000  - mov rax,gs:[00000058] { 88 }
            /// PathOfExile_x64.exe+149689 - B9 28000000           - mov ecx,00000028 { 40 }
            /// PathOfExile_x64.exe+14968E - 48 8B 00              - mov rax,[rax]
            /// PathOfExile_x64.exe+149691 - 45 33 C9              - xor r9d,r9d
            /// PathOfExile_x64.exe+149694 - BA 68020000           - mov edx,00000268 { 616 }
            /// PathOfExile_x64.exe+149699 - 44 8D 45 10           - lea r8d,[rbp+10]
            /// PathOfExile_x64.exe+14969D - 0FB7 0C 01            - movzx ecx,word ptr [rcx+rax]
            /// PathOfExile_x64.exe+1496A1 - E8 EA1A0501           - call PathOfExile_x64.exe+119B190
            /// PathOfExile_x64.exe+1496A6 - 48 8B D0              - mov rdx,rax
            ///</HowToFindIt>
            new Pattern
            (
                "Game States",
                "48 83 EC ?? 48 C7 44 24 ?? ?? ?? ?? ?? 48 89 9C 24 ?? ?? ?? ?? 48 8B F9 33 ED ?? ?? ?? ^"
            ),

            ///<HowToFindIt>
            /// NOTE: This is not a good one, it might break one day.
            /// 1: Open CheatEngine and Attach to POE Game
            /// 2: Search for String: "Mods.dat", type: UTF-16 (case sensitive is fine), Writable: false/unchecked.
            /// 3: "Find out what accesses this address"
            /// Real-Mechanism: From here, use "Break and trace instruction" and reach the instruction
            /// which loads the first address (havent tested it)
            /// 
            /// Short-cut-mechanism given below:
            /// 4: From here use 
            /// 4: Highlight one of the instruction.
            /// 5: For each static (green) address on the register RAX, RBX and etc do the following
            /// 6: Open it in "Dissect data/structure" window of CheatEngine program.
            /// 7: Go Up/Down and find a 4-byte value (HEX) 8000000C
            /// 8: Address found in step-7, do "Find out what accesses this address" on it.
            /// 9: That's your function to make the pattern of.
            /// 10: and your fileroot is valid address found in step-5 + 0x08.
            /// ==Function start==to==10 lines after== 
            /// PathOfExile_x64.exe+E17090 - 40 57                 - push rdi
            /// PathOfExile_x64.exe+E17092 - 48 83 EC 40           - sub rsp,40
            /// PathOfExile_x64.exe+E17096 - 48 C7 44 24 30 FEFFFFFF - mov qword ptr [rsp+30],FFFFFFFFFFFFFFFE
            /// PathOfExile_x64.exe+E1709F - 48 89 5C 24 58        - mov [rsp+58],rbx
            /// PathOfExile_x64.exe+E170A4 - 48 89 74 24 60        - mov [rsp+60],rsi
            /// PathOfExile_x64.exe+E170A9 - BE 40000000           - mov esi,00000040
            /// PathOfExile_x64.exe+E170AE - 65 48 8B 04 25 58000000  - mov rax,gs:[00000058]
            /// PathOfExile_x64.exe+E170B7 - 48 8B 08              - mov rcx,[rax]
            /// PathOfExile_x64.exe+E170BA - 48 8D 3D 5F894001     - lea rdi,[PathOfExile_x64.exe + 221FA20]
            /// PathOfExile_x64.exe+E170C1 - 8B 04 0E              - mov eax,[rsi + rcx]
            /// PathOfExile_x64.exe+E170C4 - 39 05 4E894001        - cmp[PathOfExile_x64.exe + 221FA18],eax
            /// PathOfExile_x64.exe+E170CA - 0F8E 9E010000         - jng PathOfExile_x64.exe+E1726E
            /// PathOfExile_x64.exe+E170D0 - 48 8D 0D 41894001     - lea rcx, [PathOfExile_x64.exe+221FA18]
            /// PathOfExile_x64.exe+E170D7 - E8 14A97000           - call PathOfExile_x64.exe+15219F0
            /// PathOfExile_x64.exe+E170DC - 83 3D 35894001 FF     - cmp dword ptr[PathOfExile_x64.exe + 221FA18],-01
            /// PathOfExile_x64.exe+E170E3 - 0F85 85010000         - jne PathOfExile_x64.exe+E1726E
            /// PathOfExile_x64.exe+E170E9 - 48 89 7C 24 50        - mov[rsp + 50], rdi
            /// PathOfExile_x64.exe+E170EE - 0F57 C0               - xorps xmm0, xmm0
            /// PathOfExile_x64.exe+E170F1 - F3 0F11 05 27894001   - movss[PathOfExile_x64.exe + 221FA20], xmm0
            ///</HowToFindIt>
            new Pattern
            (
                "File Root",
                "48 8d ?? ^ ?? ?? ?? ?? 8b 04 0e 39"
            ),

            /// <HowToFindIt>
            /// This one is really simple/old school CE formula.
            /// The only purpose of this Counter is to figure out the files-loaded-in-current-area.
            /// 1: Open CheatEngine and Attach to POE Game
            /// 2: Search for 4 bytes, Search type should be "UnknownInitialValue"
            /// 3: Now Change the Area again and again and on every area change do a "Next Scan" with "Increased Value"
            /// 4: U will find 2 green addresses at the end of that search list.
            /// 5: Pick the smaller one and create pattern for it.
            /// 5.1: Normally pattern are created by "What accesses this address/value"
            /// 5.2: but in this case the pattern at "What writes to this address/value" is more unique.
            ///
            /// NOTE: Reason you picked the smaller one in step-5 is because
            ///       the bigger one is some other number which increments by 3
            ///       every time you move from Charactor Select screen to In Game screen.
            ///       Feel free to test this, just to be sure that the Address you are picking
            ///       is the correct one.
            /// </HowToFindIt>
            new Pattern
            (
                "AreaChangeCounter",
                "E8 ?? ?? ?? ?? E8 ?? ?? ?? ?? FF 05 ^"
            ),
        };
    }
}