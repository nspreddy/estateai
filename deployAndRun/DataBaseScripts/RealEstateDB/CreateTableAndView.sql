
DROP TABLE IF EXISTS `EstateCrawlData`;

CREATE TABLE `EstateCrawlData` (
  `CrawlJobId` bigint(20) DEFAULT NULL,
  `DateCrawled` text,
  `MLSID` text,
  `Address` text,
  `City` text,
  `State` text,
  `Country` text,
  `ZipCode` int(11) DEFAULT NULL,
  `PropertyType` text,
  `Price` int(11) DEFAULT NULL,
  `PricePerSqft` int(11) DEFAULT NULL,
  `Beds` int(11) DEFAULT NULL,
  `Baths` int(11) DEFAULT NULL,
  `Sqft` int(11) DEFAULT NULL,
  `LotSize` int(11) DEFAULT NULL,
  `ParkingSpaces` int(11) DEFAULT NULL,
  `BuiltYear` int(11) DEFAULT NULL,
  `DaysInMarket` int(11) DEFAULT NULL,
  `ViewCount` int(11) DEFAULT NULL,
  `WalkScore` int(11) DEFAULT NULL,
  `Lat` int(11) DEFAULT NULL,
  `Long` int(11) DEFAULT NULL,
  `PropertyScore` int(11) DEFAULT NULL,
  `ThingsAround` text,
  `InformationSrc` text
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


DROP VIEW  IF EXISTS `FreshlyCrawledData`;

CREATE VIEW `FreshlyCrawledData` AS  SELECT  *  FROM EstateCrawlData where  CrawlJobId = (SELECT MAX(CrawlJobId) FROM  EstateCrawlData);
