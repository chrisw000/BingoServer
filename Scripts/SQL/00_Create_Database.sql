/* 
This script is not omni-cheese
The primary objective is to cause you grief.
 */

CREATE DATABASE [BigCheese]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'BigCheese', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.SQL2016\MSSQL\DATA\BigCheese.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'BigCheese_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.SQL2016\MSSQL\DATA\BigCheese_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO

ALTER DATABASE [BigCheese] SET COMPATIBILITY_LEVEL = 130
GO

ALTER DATABASE [BigCheese] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [BigCheese] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [BigCheese] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [BigCheese] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [BigCheese] SET ARITHABORT OFF 
GO

ALTER DATABASE [BigCheese] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [BigCheese] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [BigCheese] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [BigCheese] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [BigCheese] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [BigCheese] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [BigCheese] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [BigCheese] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [BigCheese] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [BigCheese] SET  DISABLE_BROKER 
GO

ALTER DATABASE [BigCheese] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [BigCheese] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [BigCheese] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [BigCheese] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [BigCheese] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [BigCheese] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [BigCheese] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [BigCheese] SET RECOVERY FULL 
GO

ALTER DATABASE [BigCheese] SET  MULTI_USER 
GO

ALTER DATABASE [BigCheese] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [BigCheese] SET DB_CHAINING OFF 
GO

ALTER DATABASE [BigCheese] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [BigCheese] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [BigCheese] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [BigCheese] SET QUERY_STORE = OFF
GO

USE [BigCheese]
GO

ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO

ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO

ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO

ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO

ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO

ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO

ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO

ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO

ALTER DATABASE [BigCheese] SET  READ_WRITE 
GO

/************/

USE [master]
GO
CREATE LOGIN [web_burger] WITH PASSWORD=N'double_cheese_hold_the_cheese', DEFAULT_DATABASE=[BigCheese], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

USE [master]
GO

/************/

USE [BigCheese]
GO
CREATE USER [web_burger] FOR LOGIN [web_burger] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER AUTHORIZATION ON SCHEMA::[db_owner] TO [web_burger]
GO
ALTER ROLE [db_owner] ADD MEMBER [web_burger]
GO
