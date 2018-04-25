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
	global $errors;

	//Comprobamos si $value es null, y añades el error diciendo el nombre de la key para saber que variable es nula.

	if(!isset($value)) 
	{
		$key = "emptyVar";

		$errorObj = array();

		$errorObj["key"] = $key;
		$errorObj["caption"] = getErrorCaption($key, $arr != null ? getKeyName($arr, $value) : $value);

		$errors[] = $errorObj;

		return true;
	}

	return false;
}

function addError($key) 
{
	global $errors;

	$params = count(func_get_args()) > 1 ? array_slice(func_get_args(), 1) : null;

	$errorObj = array();

	$errorObj["key"] = $key;
	$errorObj["caption"] = $params != null ? getErrorCaption($key, $params) : getErrorCaption($key);

	$errors[] = $errorObj;
}

function getErrorCaption($key) 
{
	global $Error;

	return StrFormat($Error[$key], array_slice(func_get_args(), 1)[0]);
}

function getErrors() 
{
    global $errors;

	return $errors;
}

// *** Core class' methods ** ///

$coreArray = array();

function showJson($data) 
{
    global $errors;

	//Prepare array...

	if(isset($errors) && count($errors) > 0)
	{
		$coreArray["errors"] = $errors;
	}
	else 
	{
		$coreArray["success"] = true;
	}

	if(isset($data) && count($data) > 0)
		$coreArray["data"] = $data;

	return json_encode($coreArray, true);
}

function PrettyDump($data)
{
 	return '<pre>' . var_export($data, true) . '</pre>';
}

function StrFormat()
{ //Realmente con esto se hace functionar mucho mas al servidor... Solamente se requiere en el logger y lo estoy usando en las consultas de SQL donde se puede hacer perfectamente un {$var}
    $args = func_get_args();

    if (count($args) == 0)
        return false;

    if (count($args) == 1)
        return $args[0];

    $str = array_shift($args);

    //die(PrettyDump($args));

    //if(is_array($args[0]))
    //	$args = $args[0];

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