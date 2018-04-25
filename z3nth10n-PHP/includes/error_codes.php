<?php

// *** Código de errores *** //

$Success = array(
    "register" => "Succesfully register",
    "login" => "Succesfully login",
    "logout" => "Succesfully logout",
    "get-profile" => "Succesfully got data",
    "get-tags" => "",
    "get-tree" => "",
    "getAppId" => "",
    "regenAuth" => "",
    "rememberAuth" => "",
    "createAuth" => "",
    "registerEntity" => "",
    "startAppSession" => "",
    "endAppSession" => ""
);

$Error = array(
    "wrong_username" => "The username called {0} hasn't been found in the DB!",
    "emptyUsername" => "",
    "forbiddenChars" => "",
    "userExists" => "",
    "shortUsername" => "",
    "longUsername" => "",
    "emptyPassword" => "",
    "shortPassword" => "",
    "wrongCPassword" => "",
    "numPassword" => "",
    "letterPassword" => "",
    "emptyEmail" => "",
    "invalidMail" => "",
    "wrongCMail" => "",
    "github_error" => "Error with Github Api",
    "error_registering_entity" => "",
    "error_updating_entity" => "",
    "error_registering_token" => "",
    "error_registering_auth" => "",
    "error_registering_account" => "",
    "wrong_app_prefix" => "... {0} ...",
    "error_starting_session" => "",
    "error_ending_session" => "",
    "error_finalizing_session" => "",
    "entity_not_exists" => "",
    "null_auth_reg" => "",
    "null_token_reg" => "",
    "null_user_reg" => "",
    "strformat_arr_empty_gaps" => "Empty arr gap #{0}",
    "strformat_str_empty_gaps" => "Empty str gap #{0}",
    "wrong_credentials" => "",
    "error_null_userdata" => "Null user data for tokenid: {0}",
    "error_auth_nullcreds" => "",
    "error_unset_parameters" => "There are unsetted parameters: {0} in the function...",
    "queryError" => "{0}",
    "phpError" => "Error: [{0}] {1}\n\nFile: {2}:{3}\n\nTrace: {4}",
    "emptyVar" => "No se ha especificado un valor para '{0}'.",
    "undefinedCase" => "Unknown '{0}' ('{1}') action!",
    "undefinedMethod" => "Unknown '{0}' method used!",
    "hackTry" => "Hack try! With the following code: {0}"
);

$EventIds = array(
    "register", "login", "logout", "get-profile", "get-tags", "get-tree", "getAppId", "regenAuth", "rememberAuth", "createAuth", "registerEntity", "startAppSession", "endAppSession", "getAppId", "get-tutorial", "get-desc"
);

$DisabledEvents = array(
    "get-tutorial", "get-desc"
);