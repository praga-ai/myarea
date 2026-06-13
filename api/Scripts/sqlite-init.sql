-- SQLite version of database schema (for local development)

-- Create Ward Table
CREATE TABLE IF NOT EXISTS Ward (
    WardId INTEGER PRIMARY KEY AUTOINCREMENT,
    WardName TEXT NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Create Part Table
CREATE TABLE IF NOT EXISTS Part (
    PartId INTEGER PRIMARY KEY AUTOINCREMENT,
    PartNumber TEXT NOT NULL,
    WardId INTEGER NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WardId) REFERENCES Ward(WardId)
);

-- Create Area Table
CREATE TABLE IF NOT EXISTS Area (
    AreaId INTEGER PRIMARY KEY AUTOINCREMENT,
    AreaName TEXT NOT NULL,
    PartId INTEGER NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PartId) REFERENCES Part(PartId)
);

-- Create Street Table
CREATE TABLE IF NOT EXISTS Street (
    StreetId INTEGER PRIMARY KEY AUTOINCREMENT,
    StreetName TEXT NOT NULL,
    AreaId INTEGER NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (AreaId) REFERENCES Area(AreaId)
);

-- Create Questionnaire Table
CREATE TABLE IF NOT EXISTS Questionnaire (
    QuestionnaireId INTEGER PRIMARY KEY AUTOINCREMENT,
    QuestionText TEXT NOT NULL,
    QuestionType TEXT NOT NULL,
    DisplayOrder INTEGER NOT NULL,
    IsActive INTEGER DEFAULT 1,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Create QuestionnaireOption Table
CREATE TABLE IF NOT EXISTS QuestionnaireOption (
    OptionId INTEGER PRIMARY KEY AUTOINCREMENT,
    QuestionnaireId INTEGER NOT NULL,
    OptionText TEXT NOT NULL,
    OptionValue TEXT NOT NULL,
    DisplayOrder INTEGER NOT NULL,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (QuestionnaireId) REFERENCES Questionnaire(QuestionnaireId)
);

-- Create Survey Table
CREATE TABLE IF NOT EXISTS Survey (
    SurveyId INTEGER PRIMARY KEY AUTOINCREMENT,
    WardId INTEGER NOT NULL,
    PartId INTEGER NOT NULL,
    AreaId INTEGER NOT NULL,
    StreetId INTEGER NOT NULL,
    SurveyData TEXT,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WardId) REFERENCES Ward(WardId),
    FOREIGN KEY (PartId) REFERENCES Part(PartId),
    FOREIGN KEY (AreaId) REFERENCES Area(AreaId),
    FOREIGN KEY (StreetId) REFERENCES Street(StreetId)
);

-- Create SurveyResponse Table
CREATE TABLE IF NOT EXISTS SurveyResponse (
    ResponseId INTEGER PRIMARY KEY AUTOINCREMENT,
    SurveyId INTEGER NOT NULL,
    QuestionnaireId INTEGER NOT NULL,
    SelectedValue TEXT,
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SurveyId) REFERENCES Survey(SurveyId),
    FOREIGN KEY (QuestionnaireId) REFERENCES Questionnaire(QuestionnaireId)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS IX_Part_WardId ON Part(WardId);
CREATE INDEX IF NOT EXISTS IX_Area_PartId ON Area(PartId);
CREATE INDEX IF NOT EXISTS IX_Street_AreaId ON Street(AreaId);
CREATE INDEX IF NOT EXISTS IX_Survey_WardId ON Survey(WardId);

-- Insert Sample Data
INSERT OR IGNORE INTO Ward (WardId, WardName) VALUES
(1, 'Ward A'),
(2, 'Ward B'),
(3, 'Ward C');

INSERT OR IGNORE INTO Part (PartId, PartNumber, WardId) VALUES
(1, 'P001', 1),
(2, 'P002', 1),
(3, 'P003', 2),
(4, 'P004', 2),
(5, 'P005', 3);

INSERT OR IGNORE INTO Area (AreaId, AreaName, PartId) VALUES
(1, 'Area 1', 1),
(2, 'Area 2', 1),
(3, 'Area 3', 2),
(4, 'Area 4', 2),
(5, 'Area 5', 3);

INSERT OR IGNORE INTO Street (StreetId, StreetName, AreaId) VALUES
(1, 'Main Street', 1),
(2, 'Oak Street', 1),
(3, 'Elm Street', 2),
(4, 'Pine Street', 2),
(5, 'Maple Street', 3);

INSERT OR IGNORE INTO Questionnaire (QuestionnaireId, QuestionText, QuestionType, DisplayOrder) VALUES
(1, 'How satisfied are you with the water supply?', 'Radio', 1),
(2, 'How satisfied are you with the street lighting?', 'Radio', 2),
(3, 'How satisfied are you with the road condition?', 'Radio', 3),
(4, 'Do you have proper waste disposal facility?', 'Radio', 4),
(5, 'How is the local healthcare facility?', 'Radio', 5);

INSERT OR IGNORE INTO QuestionnaireOption (OptionId, QuestionnaireId, OptionText, OptionValue, DisplayOrder) VALUES
(1, 1, 'Very Satisfied', 'very_satisfied', 1),
(2, 1, 'Satisfied', 'satisfied', 2),
(3, 1, 'Neutral', 'neutral', 3),
(4, 1, 'Dissatisfied', 'dissatisfied', 4),
(5, 1, 'Very Dissatisfied', 'very_dissatisfied', 5),
(6, 2, 'Very Satisfied', 'very_satisfied', 1),
(7, 2, 'Satisfied', 'satisfied', 2),
(8, 2, 'Neutral', 'neutral', 3),
(9, 2, 'Dissatisfied', 'dissatisfied', 4),
(10, 2, 'Very Dissatisfied', 'very_dissatisfied', 5),
(11, 3, 'Excellent', 'excellent', 1),
(12, 3, 'Good', 'good', 2),
(13, 3, 'Fair', 'fair', 3),
(14, 3, 'Poor', 'poor', 4),
(15, 4, 'Yes', 'yes', 1),
(16, 4, 'No', 'no', 2),
(17, 4, 'Partial', 'partial', 3),
(18, 5, 'Good', 'good', 1),
(19, 5, 'Average', 'average', 2),
(20, 5, 'Poor', 'poor', 3),
(21, 5, 'Not Available', 'not_available', 4);
