<?php

include('autoload.php');

$ret = mysqli_query($conn, "INSERT INTO hourlystats (stat_date, launcher_hits, play_hits, maxccusers) VALUES ('12-mar-2013', 0, 0, 0)");

if($ret)
	echo ":)";
else
	echo("Error description: " . mysqli_error($conn));