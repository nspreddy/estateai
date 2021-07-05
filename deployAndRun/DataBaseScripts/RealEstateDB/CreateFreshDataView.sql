
USE estateAiDb;
CREATE VIEW `FreshlyCrawledData` AS  SELECT  *  FROM EstateCrawlData where  CrawlJobId = (SELECT MAX(CrawlJobId) FROM  EstateCrawlData);


