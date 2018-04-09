<?php

// Crear la imagen
$im = imagecreatetruecolor(500, 60);

imagesavealpha($im, true);

// Crear algunos colores
$color = imagecolorallocatealpha($im, 255, 255, 255, 127);
$gris = imagecolorallocate($im, 128, 128, 128);
$negro = imagecolorallocate($im, 192, 192, 192);
//imagefilledrectangle($im, 0, 0, 399, 29, $blanco);
imagefill($im, 0, 0, $color);

// El texto a dibujar
$texto = 'Minecraft Launcher';
// Reemplace la ruta por la de su propia fuente
$fuente = 'fonts/MBold.otf';

// Añadir algo de sombra al texto
//imagettftext($im, 20, 0, 11, 21, $gris, $fuente, $texto);

// Añadir el texto
$bbox = imagettfbbox (30, 0, $fuente, $texto);

$x = $bbox[0] + (imagesx($im) / 2) - ($bbox[4] / 2) + 10;
$y = $bbox[1] + (imagesy($im) / 2) - ($bbox[5] / 2) - 5;

// Escribirlo
imagettftext($im, 30, 0, $x, $y, $negro, $fuente, $texto);

//Aquí ya hacemos el contador de visitas

//....

// Establecer el tipo de contenido
header('Content-Type: image/png');

// Usar imagepng() resultará en un texto más claro comparado con imagejpeg()
imagepng($im);
imagedestroy($im);