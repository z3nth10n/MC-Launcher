-- phpMyAdmin SQL Dump
-- version 4.7.9
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 09-04-2018 a las 09:02:16
-- Versión del servidor: 10.1.31-MariaDB
-- Versión de PHP: 5.6.34

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `z3nth10n-db`
--

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `hourlystats`
--

CREATE TABLE `hourlystats` (
  `id` int(11) NOT NULL,
  `stat_date` int(11) NOT NULL,
  `launcher_hits` int(11) NOT NULL,
  `play_hits` int(11) NOT NULL,
  `maxccusers` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `legacy_users`
--

CREATE TABLE `legacy_users` (
  `id` int(11) NOT NULL,
  `username` text NOT NULL,
  `password` text NOT NULL,
  `email` text NOT NULL,
  `receivemails` tinyint(1) NOT NULL,
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
  `ram` int(11) NOT NULL,
  `main_hdd` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `legacy_visitors`
--

CREATE TABLE `legacy_visitors` (
  `id` int(11) NOT NULL,
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
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `legacy_users`
--
ALTER TABLE `legacy_users`
  ADD PRIMARY KEY (`id`);

--
-- Indices de la tabla `legacy_visitors`
--
ALTER TABLE `legacy_visitors`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `legacy_users`
--
ALTER TABLE `legacy_users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `legacy_visitors`
--
ALTER TABLE `legacy_visitors`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
