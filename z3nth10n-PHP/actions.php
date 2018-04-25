<?php

$coreData = array();

$reqMethod = $_SERVER['REQUEST_METHOD'];
$isPost = $reqMethod === "POST";

$arr = $isPost ? @$_POST : @$_GET;

$action = $arr['action'];

$defaultCase = false;

if(!checkEmpty($arr, $action)) 
{
	switch ($reqMethod) {
		case 'POST':
			$token = @$_POST['token'];

			switch ($action) 
			{
				case 'check-user':
					//Add user if not exists
					/*
					  `username` text NOT NULL,
					  `password` text NOT NULL,
					  `email` text NOT NULL,
					  `receivemails` tinyint(1) NOT NULL,
					  `ip` varchar(15) NOT NULL, //Not parameter
					  `pcid` text NOT NULL,
					  `reg_date` date NOT NULL, //Now
					  `last_activity` date NOT NULL, //Now
					  `launcher_version` text NOT NULL,
					  `lang_used` varchar(2) NOT NULL,
					  `play_hits` int(11) NOT NULL, //Not parameter = 0
					  `launcher_hits` int(11) NOT NULL, //Not parameter = 0
					  `os` text NOT NULL,
					  `resolution` text NOT NULL,
					  `cpu_name` text NOT NULL,
					  `ram` int(11) NOT NULL,
					  `main_hdd` text NOT NULL
					*/
					break;

				case 'check-visitor':
					//Add visitor if not exists, and return data even it if exists or not
					/*
					  `ip` varchar(15) NOT NULL,
					  `pcid` text NOT NULL,
					  `reg_date` date NOT NULL,
					  `played_time` date NOT NULL,
					  `last_activity` date NOT NULL,
					  `launcher_version` text NOT NULL,
					  `lang_used` varchar(2) NOT NULL,
					  `play_hits` int(11) NOT NULL,
					  `launcher_hits` int(11) NOT NULL,
					  `os` text NOT NULL,
					  `resolution` text NOT NULL,
					  `cpu_name` text NOT NULL,
					  `ram` int(11) NOT NULL COMMENT 'In MB',
					  `main_hdd` text NOT NULL
					*/
					break;

				case 'playhit':
					//Register hit for that user for play button
					break;

				case 'launcherhit':
					//The same as before, but for launcher only
					break;

				case 'not-afk':
					//Tell the DB we aren't offline and update current value of online users in DB (actual_ccusers)
					//and if we are playing update played time from user (we have to detect if we are playing)
					break;

				default:
					$defaultCase = true;
					break;
			}
			break;

		case 'GET':
			switch ($action) 
			{
				case 'secret':
					//We get here the token for every petition
					//First step: give a random string and store it on a SecureString (C#)
					//Second step: Generate a token in both (client & server) and validate client token with server one
				
					//Secret would be random string given from the first step of this case
					$coreData["secret"] = bin2hex(openssl_random_pseudo_bytes(16));

					//I should use free https to avoid security problems through sniffing packets
					break;

				default:
					$defaultCase = true;
					break;
			}
			break;
		
		default:
			addError("undefinedMethod", $reqMethod);
			break;
	}

	if($defaultCase)
		addError("undefinedCase", $action, $reqMethod);
}
else 
{
	die("Action is null!");
}