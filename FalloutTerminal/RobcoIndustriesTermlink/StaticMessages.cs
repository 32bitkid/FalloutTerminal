using System;
using System.Collections.Generic;
using System.Text;
using FalloutTerminal.Communications;

namespace FalloutTerminal.RobcoIndustriesTermlink
{
	public abstract class StaticMessages {
        public const string ComputersBack = "Back";
        public const string ComputersHeader1 = "ROBCO INDUSTRIES UNIFIED OPERATING SYSTEM";
        public const string ComputersHeader2 = "COPYRIGHT 2075-2077 ROBCO INDUSTRIES";
        public const string ComputersLogon = "LOGON ADMIN";
        public const string ComputersWelcome = "Welcome to the service on SERVER1";
        public const string HackingAccessing1 = "Please wait";
        public const string HackingAccessing2 = "while system";
        public const string HackingAccessing3 = "is accessed.";
        public const string HackingCorrect = "correct";
        public const string HackingDenied = "Entry denied";
        public const string HackingDudRemoved = "Dud removed.";
        public const string HackingGranted = "Exact match!";
        public const string HackingHeader = "ROBCO INDUSTRIES (TM) TERMLINK PROTOCOL";
        public const string HackingHeader2 = "ENTER PASSWORD NOW";
        public const string HackingHeader3 = "ATTEMPT(S) LEFT:";
        public const string HackingIntro02 = "SET TERMINAL/INQUIRE";
        public const string HackingIntro03 = "\r\nRIT-V300\r\n\r\n";
        public const string HackingIntro04 = "SET FILE/PROTECTION=OWNER:RWED ACCOUNTS.F";
        public const string HackingIntro05 = "SET HALT RESTART/MAINT";
        public const string MaintainenceModeBootMessage = "\r\nInitializing Robco Industries(TM) MF Boot Agent v2.3.0\r\nRETROS BIOS\r\nRBIOS-4.02.08.00 52EE5.E7.E8\r\nCopyright 2201-2203 Robco Ind.\r\nUppermem: 64 KB\r\nRoot (5A8)\r\nMaintenance Mode\r\n\r\n";
        public const string HackingIntro13 = "RUN DEBUG/ACCOUNTS.F";
        public const string HackingLockout1 = "Lockout in";
        public const string HackingLockout2 = "progress.";
        public const string HackingLockout3 = "TERMINAL LOCKED";
        public const string HackingLockout4 = "PLEASE CONTACT AN ADMINISTRATOR";
        public const string HackingToleranceReset1 = "Allowance";
        public const string HackingToleranceReset2 = "replenished.";
        public const string HackingWarning = "!!! WARNING: LOCKOUT IMMINENT !!!";
        public const string TerminalLocked = "You cannot hack this computer.";
        public const string TerminalServerText1 = "-Server 1-";
        public const string TerminalServerText10 = "-Server 10-";
        public const string TerminalServerText2 = "-Server 2-";
        public const string TerminalServerText3 = "-Server 3-";
        public const string TerminalServerText4 = "-Server 4-";
        public const string TerminalServerText5 = "-Server 5-";
        public const string TerminalServerText6 = "-Server 6-";
        public const string TerminalServerText7 = "-Server 7-";
        public const string TerminalServerText8 = "-Server 8-";
        public const string TerminalServerText9 = "-Server 9-";
		public const string BadCommand = "BAD COMMAND OR FILE NAME\r\n\r\n";
		public const string AccessDenied = "ACCESS DENIED\r\n\r\n";

        public static readonly string NormalBootMessage = IBM3151.Commands.ClearAll + IBM3151.Commands.Beep + "WELCOME TO ROBCO INDUSTRIES (TM) TERMLINK\r\n\n";
	}
}
