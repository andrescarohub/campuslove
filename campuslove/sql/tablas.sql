-- Crear la base de datos si no existe

DROP database CampusLoveDB;


CREATE DATABASE IF NOT EXISTS CampusLoveDB;




USE CampusLoveDB;

-- Tabla de Usuarios
CREATE TABLE Usuarios (
    UsuarioID INT PRIMARY KEY AUTO_INCREMENT,
    Nombre VARCHAR(100) NOT NULL UNIQUE, -- Nombre único para facilitar el "login" por nombre
    Edad INT NOT NULL,
    Genero VARCHAR(50) NOT NULL, -- Podría ser un ENUM si los géneros son fijos
    Intereses TEXT, -- Almacenaremos como CSV: "Futbol,Musica,Leer"
    Carrera VARCHAR(100),
    FrasePerfil TEXT,
    CreditosLikesDiarios INT DEFAULT 10, -- Límite de likes por día
    UltimoReinicioCreditos DATE, -- Para saber cuándo se reiniciaron los créditos
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Tabla de Interacciones (Likes/Dislikes)
CREATE TABLE Interacciones (
    InteraccionID INT PRIMARY KEY AUTO_INCREMENT,
    UsuarioOrigenID INT NOT NULL, -- Quién realiza la acción
    UsuarioDestinoID INT NOT NULL, -- Sobre quién se realiza la acción
    TipoInteraccion ENUM('Like', 'Dislike') NOT NULL,
    FechaInteraccion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UsuarioOrigenID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    FOREIGN KEY (UsuarioDestinoID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    CONSTRAINT UQ_InteraccionUnica UNIQUE (UsuarioOrigenID, UsuarioDestinoID) -- Un usuario solo puede interactuar una vez con otro
);

-- Tabla de Matches (Coincidencias)
CREATE TABLE Matches (
    MatchID INT PRIMARY KEY AUTO_INCREMENT,
    Usuario1ID INT NOT NULL,
    Usuario2ID INT NOT NULL,
    FechaMatch DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (Usuario1ID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    FOREIGN KEY (Usuario2ID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    -- Para evitar duplicados (A-B es igual a B-A) y asegurar que Usuario1ID < Usuario2ID
    CONSTRAINT UQ_MatchUnico CHECK (Usuario1ID < Usuario2ID),
    CONSTRAINT UQ_MatchParUnico UNIQUE (Usuario1ID, Usuario2ID)
);

-- Índices para optimizar búsquedas frecuentes
CREATE INDEX IDX_Interacciones_Origen_Destino ON Interacciones(UsuarioOrigenID, UsuarioDestinoID);
CREATE INDEX IDX_Matches_Usuario1 ON Matches(Usuario1ID);
CREATE INDEX IDX_Matches_Usuario2 ON Matches(Usuario2ID);