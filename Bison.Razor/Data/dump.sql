INSERT INTO user VALUES(1,'Eduard','edka@itu.dk');
INSERT INTO user VALUES(2,'Peter','debloh@itu.dk');

INSERT INTO observation VALUES(1,1,'A heron',1690892208);
INSERT INTO observation VALUES(2,2,'A big bird',1690895308);

INSERT INTO comment (comment_id, observation_id, author, text, pub_date)
VALUES(1, 1,'Rats', 'wow amazing',1690892208);

INSERT INTO proposal (proposal_id, observation_id, author, taxon_id, pub_date)
VALUES (1, 1, 'Rats','MSTSNM:Arter:a15367e4-f785-ea11-aa77-501ac539d1ea', 1690895308);
