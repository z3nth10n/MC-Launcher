<?php
/**
 * Created by PhpStorm.
 * User: Álvaro
 * Date: 07/05/2018
 * Time: 1:43
 */

// Función para analizar la cabecera de autenticación HTTP
function analizar_http_digest($txt)
{
    // Protección contra datos ausentes
    $partes_necesarias = array('nonce'=>1, 'nc'=>1, 'cnonce'=>1, 'qop'=>1, 'username'=>1, 'uri'=>1, 'response'=>1);
    $datos = array();
    $claves = implode('|', array_keys($partes_necesarias));

    preg_match_all('@(' . $claves . ')=(?:([\'"])([^\2]+?)\2|([^\s,]+))@', $txt, $coincidencias, PREG_SET_ORDER);

    foreach ($coincidencias as $c) {
        $datos[$c[1]] = $c[3] ? $c[3] : $c[4];
        unset($partes_necesarias[$c[1]]);
    }

    return $partes_necesarias ? false : $datos;
}

$dominio = 'Restricted area';

// usuario => contraseña
$usuarios = array('root' => '1234');


if (empty($_SERVER['PHP_AUTH_DIGEST'])) {
    header('HTTP/1.1 401 Unauthorized');
    header('WWW-Authenticate: Digest realm="'.$dominio.
        '",qop="auth",nonce="'.uniqid().'",opaque="'.md5($dominio).'"');

    die('Not authorized (cancelled)');
}


// Analizar la variable PHP_AUTH_DIGEST
if (!($datos = analizar_http_digest($_SERVER['PHP_AUTH_DIGEST'])) ||
    !isset($usuarios[$datos['username']]))
    die('Incorrect credentials');


// Generar una respuesta válida
$A1 = md5($datos['username'] . ':' . $dominio . ':' . $usuarios[$datos['username']]);
$A2 = md5($_SERVER['REQUEST_METHOD'].':'.$datos['uri']);
$respuesta_válida = md5($A1.':'.$datos['nonce'].':'.$datos['nc'].':'.$datos['cnonce'].':'.$datos['qop'].':'.$A2);

if ($datos['response'] != $respuesta_válida)
    die('Credenciales incorrectas');

// Todo bien, ... continuamos...
