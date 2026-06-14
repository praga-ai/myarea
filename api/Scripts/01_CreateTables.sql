-- Create Ward Table
CREATE TABLE Ward (
    WardId INT PRIMARY KEY IDENTITY(1,1),
    WardName NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME DEFAULT GETUTCDATE()
);

-- Create Part Table
CREATE TABLE Part (
    PartId INT PRIMARY KEY IDENTITY(1,1),
    PartNumber NVARCHAR(50) NOT NULL,
    WardId INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (WardId) REFERENCES Ward(WardId)
);

-- Create Area Table
CREATE TABLE Area (
    AreaId INT PRIMARY KEY IDENTITY(1,1),
    AreaName NVARCHAR(100) NOT NULL,
    PartId INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (PartId) REFERENCES Part(PartId)
);

-- Create Street Table
CREATE TABLE Street (
    StreetId INT PRIMARY KEY IDENTITY(1,1),
    StreetName NVARCHAR(100) NOT NULL,
    AreaId INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (AreaId) REFERENCES Area(AreaId)
);

-- Create Questionnaire Table
CREATE TABLE Questionnaire (
    QuestionnaireId INT PRIMARY KEY IDENTITY(1,1),
    QuestionText NVARCHAR(500) NOT NULL,
    QuestionType NVARCHAR(50) NOT NULL, -- 'Radio', 'Text', 'Checkbox', etc.
    DisplayOrder INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETUTCDATE()
);

-- Create QuestionnaireOption Table
CREATE TABLE QuestionnaireOption (
    OptionId INT PRIMARY KEY IDENTITY(1,1),
    QuestionnaireId INT NOT NULL,
    OptionText NVARCHAR(200) NOT NULL,
    OptionValue NVARCHAR(100) NOT NULL,
    DisplayOrder INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (QuestionnaireId) REFERENCES Questionnaire(QuestionnaireId)
);

-- Create Survey Table (Modified)
CREATE TABLE Survey (
    SurveyId INT PRIMARY KEY IDENTITY(1,1),
    WardId INT NOT NULL,
    PartId INT NOT NULL,
    AreaId INT NOT NULL,
    StreetId INT NOT NULL,
    SurveyData NVARCHAR(MAX), -- JSON storing questionnaire responses
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (WardId) REFERENCES Ward(WardId),
    FOREIGN KEY (PartId) REFERENCES Part(PartId),
    FOREIGN KEY (AreaId) REFERENCES Area(AreaId),
    FOREIGN KEY (StreetId) REFERENCES Street(StreetId)
);

-- Create SurveyResponse Table (for individual question responses)
CREATE TABLE SurveyResponse (
    ResponseId INT PRIMARY KEY IDENTITY(1,1),
    SurveyId INT NOT NULL,
    QuestionnaireId INT NOT NULL,
    SelectedValue NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (SurveyId) REFERENCES Survey(SurveyId),
    FOREIGN KEY (QuestionnaireId) REFERENCES Questionnaire(QuestionnaireId)
);

-- Create indexes for performance
CREATE INDEX IX_Part_WardId ON Part(WardId);
CREATE INDEX IX_Area_PartId ON Area(PartId);
CREATE INDEX IX_Street_AreaId ON Street(AreaId);
CREATE INDEX IX_Survey_WardId ON Survey(WardId);

-- Insert Sample Data
INSERT INTO Ward (WardName) VALUES ('Ward A'), ('Ward B'), ('Ward C');

INSERT INTO Part (PartNumber, WardId) VALUES
('P001', 1), ('P002', 1), ('P003', 2), ('P004', 2), ('P005', 3);

INSERT INTO Area (AreaName, PartId) VALUES
('Area 1', 1), ('Area 2', 1), ('Area 3', 2), ('Area 4', 2), ('Area 5', 3);

INSERT INTO Street (StreetName, AreaId) VALUES
('Main Street', 1), ('Oak Street', 1), ('Elm Street', 2), ('Pine Street', 2), ('Maple Street', 3);

-- Insert Sample Questionnaires
INSERT INTO Questionnaire (QuestionText, QuestionType, DisplayOrder) VALUES
('How satisfied are you with the water supply?', 'Radio', 1),
('How satisfied are you with the street lighting?', 'Radio', 2),
('How satisfied are you with the road condition?', 'Radio', 3),
('Do you have proper waste disposal facility?', 'Radio', 4),
('How is the local healthcare facility?', 'Radio', 5);

-- Insert Sample Questionnaire Options
INSERT INTO QuestionnaireOption (QuestionnaireId, OptionText, OptionValue, DisplayOrder) VALUES
-- Q1 Options
(1, 'Very Satisfied', 'very_satisfied', 1),
(1, 'Satisfied', 'satisfied', 2),
(1, 'Neutral', 'neutral', 3),
(1, 'Dissatisfied', 'dissatisfied', 4),
(1, 'Very Dissatisfied', 'very_dissatisfied', 5),
-- Q2 Options
(2, 'Very Satisfied', 'very_satisfied', 1),
(2, 'Satisfied', 'satisfied', 2),
(2, 'Neutral', 'neutral', 3),
(2, 'Dissatisfied', 'dissatisfied', 4),
(2, 'Very Dissatisfied', 'very_dissatisfied', 5),
-- Q3 Options
(3, 'Excellent', 'excellent', 1),
(3, 'Good', 'good', 2),
(3, 'Fair', 'fair', 3),
(3, 'Poor', 'poor', 4),
-- Q4 Options
(4, 'Yes', 'yes', 1),
(4, 'No', 'no', 2),
(4, 'Partial', 'partial', 3),
-- Q5 Options
(5, 'Good', 'good', 1),
(5, 'Average', 'average', 2),
(5, 'Poor', 'poor', 3),
(5, 'Not Available', 'not_available', 4);
