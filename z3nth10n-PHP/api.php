<?php

include('autoload.php');

$isPost = isset($_POST);

$action = $isPost ? @$_POST['action'] : @$_GET['action'];

if(isset($action)) 
{
	if($isPost) 
	{
		$token = @$_POST['token'];
		switch ($action) 
		{
			case 'add-user':
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
				# code...
				break;
		}
	}
	else 
	{
		switch ($action) 
		{
			case 'get-token':
				//Idk how I will generate a safe token, but for now, with a rnd I will output this token and store it in DB
				break;
			
			default:
				# code...
				break;
		}
	}
}