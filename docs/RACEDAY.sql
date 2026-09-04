CREATE DATABASE RACEDAY;
USE [RACEDAY];

--create event organiser table--
CREATE TABLE EventOrganiser(
organiserID INT IDENTITY (200,1) PRIMARY KEY,
firstName VARCHAR(50) NOT NULL,
lastName VARCHAR(50) NOT NULL,
email VARCHAR(100) NOT NULL UNIQUE, -- ORGANISERS CAN'T HAVE THE SAME EMAILL THAT IS WHY WE USE UNIQUE--
password VARCHAR(250) NOT NULL
);
SELECT * FROM EventOrganiser;

--create participant table--
CREATE TABLE Participant(
partcipantID INT IDENTITY(1048330,1) PRIMARY KEY,
firstName VARCHAR(50) NOT NULL,
lastName VARCHAR(50) NOT NULL,
email VARCHAR(100) NOT NULL UNIQUE,
password VARCHAR(255) NOT NULL
);

CREATE TABLE Event(
eventID INT IDENTITY(300,1) PRIMARY KEY,
eventName VARCHAR(100) NOT NULL,
eventDate DATE NOT NULL,
location VARCHAR(150) NOT NULL,
eventType VARCHAR(50) NOT NULL,
description VARCHAR(500),
organiserID INT NOT NULL,

CONSTRAINT FK_Event_Organiser FOREIGN KEY (organiserID) REFERENCES EventOrganiser(organiserID)
);

CREATE TABLE Category(
categoryID INT IDENTITY(400,1) PRIMARY KEY,
categoryName VARCHAR(100) NOT NULL,
distance DECIMAL(5,2) NOT NULL,
eventID INT NOT NULL,

CONSTRAINT FK_Category_Event FOREIGN KEY (eventID) REFERENCES Event(eventID)
);

CREATE TABLE Enrolment
(enrolmentID INT IDENTITY(5000,1) PRIMARY KEY,
participantID INT NOT NULL,
categoryID INT NOT NULL,
enrolmentDate DATE NOT NULL

CONSTRAINT DF_Enrolment_Date DEFAULT GETDATE(), status VARCHAR(20) NOT NULL
CONSTRAINT DF_Enrolment_Status DEFAULT 'Pending',

CONSTRAINT CK_Enrolment_Status
CHECK (status IN ('Pending', 'Confirmed', 'Rejected')),

CONSTRAINT FK_Enrolment_Participant FOREIGN KEY (participantID) REFERENCES Participant(partcipantID),
CONSTRAINT FK_Enrolment_Category
FOREIGN KEY (categoryID)
REFERENCES Category(categoryID),

CONSTRAINT UQ_Enrolment_Participant_Category UNIQUE (participantID, categoryID)
);

CREATE TABLE Result(
resultID INT IDENTITY(1,1) PRIMARY KEY,
enrolmentID INT NOT NULL UNIQUE,
finishTime TIME NOT NULL,
position INT NOT NULL,

CONSTRAINT FK_Result_Enrolment FOREIGN KEY (enrolmentID) REFERENCES Enrolment(enrolmentID)
);

--inser values into the tables--

--event orgainser table--
INSERT INTO EventOrganiser (firstName, lastName, email, password)
VALUES
('Matamela', 'Nesidoni', 'matamela@raceday.co.za', 'Matamela.22@'),
('Ndiwashu', 'Nkosi', 'ndiwashu19@raceday.co.za', 'Ndiwa_19');

SELECT * FROM EventOrganiser;

--insert 2 participants in the participant table--
INSERT INTO Participant(firstName, lastName, email, password)
VALUES
('Amanda', 'Sereni', 'serenia@gmail.com', 'amanda_22'),
('Kabelo', 'Mulaudzi', 'mulaudzikb34@gmail.com', 'Kabelo@366');

SELECT * FROM Participant;

--insert values into event table--
INSERT INTO Event(eventName, eventDate, location, eventType, description, organiserID)
VALUES('Joburg Spring Half Marathon','2026-10-18','Johannesburg CBD and Northern Suburbs, Gauteng','Running','A popular urban half marathon showcasing Johannesburg''s vibrant running community with a scenic route through the city''s business district and leafy northern suburbs.',200),
('Soweto Cycling Challenge','2026-11-08','Soweto, Gauteng','Cycling','A community-driven cycling event celebrating the cycling culture of Soweto with routes for all fitness levels through historic and modern neighbourhoods.',201),
('Freedom Park Heritage Run','2026-11-22','Freedom Park, Pretoria, Gauteng','Running','A running event raising funds for heritage conservation, featuring a scenic route through the iconic Freedom Park precinct in Pretoria.',200);
SELECT * FROM Event;

--insert values into category table--
INSERT INTO Category (categoryName, distance, eventID)
VALUES
-- Joburg Spring Half Marathon
('10km Fun Run', 10.00, 302),
('21km Half Marathon', 21.00, 302),

-- Soweto Cycling Challenge
('40km Cycle Challenge', 40.00, 303),
('80km Cycle Challenge', 80.00, 303),

-- Freedom Park Heritage Run
('5km Heritage Run', 5.00, 304),
('10km Heritage Run', 10.00, 304);
SELECT * FROM Category;

--insert values into the enrolment table--
INSERT INTO Enrolment
(participantID, categoryID, enrolmentDate, status)
VALUES
-- Amanda Sereni
(1048330, 404, '2026-09-01', 'Confirmed'),
(1048330, 405, '2026-09-01', 'Confirmed'),

-- Kabelo Mulaudzi
(1048331, 406, '2026-09-02', 'Confirmed'),
(1048331, 408, '2026-09-02', 'Pending');

SELECT * FROM Enrolment;

--insert values into the results table--
INSERT INTO Result (enrolmentID, finishTime, position)
VALUES(5004, '01:48:22', 15),
(5005, '02:05:33', 22),
(5006, '01:35:10', 8);

SELECT * FROM Result;
