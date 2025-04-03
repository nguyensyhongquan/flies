Create table Users(
	[user_id] int identity(1,1) primary key,
	email varchar(100) not null unique ,
	passwordhash varchar(255) not null,
	fullname varchar(100) ,
	birthday DateTime,
	Gender CHAR(1) CHECK (Gender IN ('M', 'F', 'O')),
	 _address nvarchar(100),
	PhoneNumber VARCHAR(20) UNIQUE
	avatar_url varchar(255) ,
	 [Role] NVARCHAR(50) CHECK (Role IN ('admin', 'mentor', 'student')) NOT NULL,
	 Balance DECIMAL(18,2) DEFAULT 0.00,
	 CreatedAt DATETIME DEFAULT GETDATE() Null,
	   UpdatedAt DATETIME DEFAULT GETDATE(),
	   LastLogin DATETIME NULL,
	   [Status] NVARCHAR(50) CHECK (Status IN ('active', 'blocked')) DEFAULT 'active'

)
