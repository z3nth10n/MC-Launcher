<?php

//Functions

$errors = array();

function getKeyName($arr, $value) 
{
    $key = array_search ($value, $arr);
    return $key;
}

function checkEmpty($arr, $value) 
{
	//Comprobamos si $value es null, y añades el error diciendo el nombre de la key para saber que variable es nula.
}

//Tengo que buscar la API que hice en su momento para gestionar los errores, en algun sitio hice el tema de las captions