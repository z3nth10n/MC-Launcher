<?php

ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

include('Settings.php');

$conn = mysqli_connect($dbserver, $dbuser, $dbpass, $dbname);

include('core.php');