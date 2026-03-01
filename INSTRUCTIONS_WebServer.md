===========================================================================
DREAMSLIVE SOLUTIONS - WEB SERVER SETUP & DIAGNOSTICS GUIDE
===========================================================================

This document contains the essential Command Prompt (CMD) instructions 
required to allow the Presenter App to communicate over a local network 
without requiring Administrative privileges every time the app runs.

---------------------------------------------------------------------------
1. INITIAL SETUP (RUN ONCE PER MACHINE)
---------------------------------------------------------------------------
The following command "reserves" the port for the application. 
Must be run in COMMAND PROMPT (ADMINISTRATOR).

Command:
  netsh http add urlacl url=http://*:21011/ user=Everyone

Verification:
  netsh http show urlacl url=http://*:21011/
  (Look for "User: \Everyone" in the output)

---------------------------------------------------------------------------
2. NETWORK DIAGNOSTICS (TESTING THE CONNECTION)
---------------------------------------------------------------------------
If the phone/browser cannot connect, run these in a standard CMD window:

A. Check if the App is actually listening:
   Command: netstat -ano | findstr :21011
   Result:  If you see "LISTENING", the C# code is running correctly.

B. Find your Local IP Address:
   Command: ipconfig
   Result:  Look for "IPv4 Address" (e.g., 192.168.1.XX). 
            This is the address to type into the phone's browser.

C. Test Ping (From another PC on the same Wi-Fi):
   Command: ping [Your_IP_Address]
   Result:  If "Timed Out", the Windows Firewall is likely blocking traffic.

---------------------------------------------------------------------------
3. FIREWALL EXCEPTIONS (IF PING FAILS)
---------------------------------------------------------------------------
If the app is "LISTENING" but the phone still can't connect, run this 
in COMMAND PROMPT (ADMINISTRATOR) to open the port:

Command:
  netsh advfirewall firewall add rule name="Presenter App Server" dir=in action=allow protocol=TCP localport=21011

---------------------------------------------------------------------------
4. RESET / UNINSTALL
---------------------------------------------------------------------------
To remove the network reservation and cleanup settings:

Command:
  netsh http delete urlacl url=http://*:21011/

---------------------------------------------------------------------------
5. QUICK TROUBLESHOOTING CHECKLIST
---------------------------------------------------------------------------
[ ] Is the PC and Phone on the SAME Wi-Fi network?
[ ] Is the Windows Network Profile set to "Private" (not Public)?
[ ] Did you run the 'netsh' command in step 1 as Admin?
[ ] Does the C# app show the URL as http://[Your_IP]:21011?

===========================================================================