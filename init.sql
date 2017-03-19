/* Создание древовидной таблицы */
CREATE TABLE IF NOT EXISTS dummy (
    id INTEGER NOT NULL,
    id_parent INTEGER NOT NULL,
    name TEXT NOT NULL,
    discount REAL NOT NULL,
    independent BOOLEAN DEFAULT 0 CHECK(independent IN (0, 1)),
    description VARCHAR(124) DEFAULT NULL,
    CONSTRAINT pk_dummy PRIMARY KEY(id)
);

INSERT INTO dummy VALUES(1, 0, "Миасс",   4, 1, NULL);
INSERT INTO dummy VALUES(2, 1, "Амелия",  5, 0, NULL);
INSERT INTO dummy VALUES(3, 2, "Тест1",   2, 0, NULL);
INSERT INTO dummy VALUES(4, 1, "Тест2",   0, 0, NULL);
INSERT INTO dummy VALUES(5, 0, "Курган", 11, 1, NULL);

/* Создание таблицы тестов */
CREATE TABLE IF NOT EXISTS tests (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    choose TEXT NOT NULL,
    discount REAL NOT NULL,
    discount_parent REAL NOT NULL,
    price REAL NOT NULL,
    result REAL NOT NULL
);
