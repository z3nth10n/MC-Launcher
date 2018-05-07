<?php

//Iniciamos errores

include('autoload.php');

include('actions.php');

//actions_main($coreData);

header('Content-Type: application/json');
die(showJson($coreData));

//Cerramos y mostramos errores