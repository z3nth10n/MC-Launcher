<?php

include('includes/error_codes.php');

//Functions

// *** Error class' methods ** ///

$errors = array();

function getKeyName($arr, $value) 
{
	$key = array_search($value, $arr);
	return $key;
}

function checkEmpty($arr, $value) 
{
	//Comprobamos si $value es null, y añades el error diciendo el nombre de la key para saber que variable es nula.

	if(!isset($value)) 
	{
		$key = "emptyVar";
		$errors["key"] = $key;
		$errors["caption"] = getErrorCaption($key, $arr != null ? getKeyName($arr, $value) : $value);

		return true;
	}

	return false;
}

function getErrorCaption($key, $vars) 
{
	global $Error;

	return StrFormat($Error[$key], array_slice(func_get_args(), 1));
}

function getErrors() 
{
	return $errors;
}

// *** Core class' methods ** ///

$coreArray = array();

function showJson($data) 
{
	//Prepare array...

	if(isset($errors)) 
	{
		$coreArray["errors"] = $errors;
	}
	else 
	{
		$coreArray["success"] = true;
	}

	if(isset($data))
		$coreArray["data"] = $data;

	return json_encode($coreArray, true);
}

function StrFormat()
{ //Realmente con esto se hace functionar mucho mas al servidor... Solamente se requiere en el logger y lo estoy usando en las consultas de SQL donde se puede hacer perfectamente un {$var}
    $args = func_get_args();

    if (count($args) == 0)
        return false;

    if (count($args) == 1)
        return $args[0];

    $str = array_shift($args);

    if(count($args) == 2 && is_array($args[0]))
        $str = $args[0];
    else if(count($args) > 2 && is_array($args[0]))
        die("If you pass the second parameter as an array, you can't pass more parameters to this function.");

    $str = preg_replace_callback('/\\{(0|[1-9]\\d*)\\}/', function($match) use($args, $str)
    {
        $trace = debug_backtrace();
        if(is_array($args[0]) && empty($args[0][$match[1]]))
            return $trace[2]["function"] == "StrFormat" && !checkEmpty('strformat_arr_empty_gaps', $match[1]);

        return isset($args[0]) && is_array($args[0]) && isset($match[1]) ? $args[0][$match[1]] : $args[$match[1]];
    }, $str);

    return $str;
}