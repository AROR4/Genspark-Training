CREATE OR REPLACE FUNCTION
get_most_borrowed_books()
RETURNS TABLE
(
    "BookId" INT,
    "Title" VARCHAR,
    "Author" VARCHAR,
    "BorrowCount" BIGINT
)
LANGUAGE plpgsql
AS
$$
BEGIN

    RETURN QUERY

    SELECT
        b."BookId",
        b."Title",
        b."Author",

        COUNT(br."BorrowingId")
            AS "BorrowCount"

    FROM "Borrowings" br

    JOIN "BookCopies" bc
        ON br."BookCopyId" =
           bc."BookCopyId"

    JOIN "Books" b
        ON bc."BookId" =
           b."BookId"

    GROUP BY
        b."BookId",
        b."Title",
        b."Author"

    ORDER BY
        "BorrowCount" DESC,
        b."Title";

END;
$$;


CREATE OR REPLACE FUNCTION
get_members_with_pending_fines()
RETURNS TABLE
(
    "MemberId" INT,
    "MemberName" VARCHAR,
    "PendingFine" DECIMAL
)
LANGUAGE plpgsql
AS
$$
BEGIN

    RETURN QUERY

    SELECT
        m."MemberId",
        m."FullName",

        SUM(f."FineAmount")::DECIMAL
            AS "PendingFine"

    FROM "Fines" f

    JOIN "Borrowings" br
        ON f."BorrowingId" =
           br."BorrowingId"

    JOIN "Members" m
        ON br."MemberId" =
           m."MemberId"

    WHERE
        f."IsPaid" = false

    GROUP BY
        m."MemberId",
        m."FullName"

    HAVING
        SUM(f."FineAmount") > 0

    ORDER BY
        "PendingFine" DESC;

END;
$$;

CREATE OR REPLACE FUNCTION
get_member_borrowing_summary(
    p_member_id INT
)
RETURNS TABLE
(
    "TotalBorrowedBooks" BIGINT,
    "ReturnedBooks" BIGINT,
    "PendingFine" DECIMAL
)
LANGUAGE plpgsql
AS
$$
BEGIN

    RETURN QUERY

    SELECT

        COUNT(br."BorrowingId")
            AS "TotalBorrowedBooks",

        COUNT(
            CASE
                WHEN br."ReturnDate" IS NOT NULL
                THEN 1
            END
        ) AS "ReturnedBooks",

        COALESCE(
            SUM(
                CASE
                    WHEN f."IsPaid" = false
                    THEN f."FineAmount"
                    ELSE 0
                END
            ),
            0
        ) AS "PendingFine"

    FROM "Borrowings" br

    LEFT JOIN "Fines" f
        ON br."BorrowingId" = f."BorrowingId"

    WHERE br."MemberId" = p_member_id;

END;
$$;

CREATE OR REPLACE FUNCTION
get_overdue_books_report()
RETURNS TABLE
(
    "BorrowingId" INT,
    "MemberName" VARCHAR,
    "BookTitle" VARCHAR,
    "DueDate" TIMESTAMP,
    "DelayedDays" INT
)
LANGUAGE plpgsql
AS
$$
BEGIN

    RETURN QUERY

    SELECT
        br."BorrowingId",
        m."FullName",
        b."Title",
        br."DueDate",

        (CURRENT_DATE - br."DueDate"::DATE)::INT
            AS "DelayedDays"

    FROM "Borrowings" br

    JOIN "Members" m
        ON br."MemberId" = m."MemberId"

    JOIN "BookCopies" bc
        ON br."BookCopyId" = bc."BookCopyId"

    JOIN "Books" b
        ON bc."BookId" = b."BookId"

    WHERE
        br."ReturnDate" IS NULL
        AND br."DueDate"::DATE < CURRENT_DATE

    ORDER BY
        "DelayedDays" DESC;

END;
$$;



CREATE OR REPLACE FUNCTION
process_book_return(
    p_borrowing_id INT,
    p_new_damage_percentage INT
)
RETURNS DECIMAL
LANGUAGE plpgsql
AS
$$
DECLARE

    due_date DATE;

    delayed_days INT := 0;

    late_fine DECIMAL := 0;
    damage_fine DECIMAL := 0;
    total_fine DECIMAL := 0;

    old_damage_percentage INT;

    increased_damage INT := 0;

    book_price DECIMAL;

    v_book_copy_id INT;

BEGIN

    -- Get Borrowing + Book Details

    SELECT
        br."DueDate"::DATE,
        bc."DamagePercentage",
        bk."Price",
        bc."BookCopyId"

    INTO
        due_date,
        old_damage_percentage,
        book_price,
        v_book_copy_id

    FROM "Borrowings" br

    JOIN "BookCopies" bc
        ON br."BookCopyId" =
           bc."BookCopyId"

    JOIN "Books" bk
        ON bc."BookId" =
           bk."BookId"

    WHERE br."BorrowingId" =
          p_borrowing_id;

    -- Borrowing validation

    IF NOT FOUND THEN
        RAISE EXCEPTION
            'Borrowing not found';
    END IF;

    -- Calculate delayed days

    delayed_days :=
        CURRENT_DATE - due_date;

    IF delayed_days > 0 THEN
        late_fine :=
            delayed_days * 10;
    ELSE
        delayed_days := 0;
    END IF;

    -- Calculate damage increase

    increased_damage :=
        p_new_damage_percentage -
        old_damage_percentage;

    -- Damage fine logic

    IF increased_damage > 0 THEN

        IF p_new_damage_percentage >= 100 THEN

            damage_fine :=
                book_price;

        ELSIF increased_damage >= 75 THEN

            damage_fine := 500;

        ELSIF increased_damage >= 50 THEN

            damage_fine := 300;

        ELSIF increased_damage >= 25 THEN

            damage_fine := 100;

        END IF;

    END IF;

    -- Total fine

    total_fine :=
        late_fine + damage_fine;

    -- Update Borrowing

    UPDATE "Borrowings"

    SET
        "ReturnDate" = CURRENT_DATE,
        "Status" = 'Returned'

    WHERE "BorrowingId" =
          p_borrowing_id;

    -- Update BookCopy

    UPDATE "BookCopies"

    SET
        "DamagePercentage" =
            p_new_damage_percentage,

        "IsAvailable" =
            CASE
                WHEN p_new_damage_percentage >= 100
                THEN false
                ELSE true
            END,

        "Status" =
            CASE
                WHEN p_new_damage_percentage >= 100
                THEN 'Lost'
                ELSE 'Available'
            END

    WHERE "BookCopyId" =
          v_book_copy_id;

    -- Create Fine if needed

    IF total_fine > 0 THEN

        INSERT INTO "Fines"
        (
            "BorrowingId",
            "FineAmount",
            "IsPaid",
            "CreatedDate"
        )
        VALUES
        (
            p_borrowing_id,
            total_fine,
            false,
            CURRENT_TIMESTAMP
        );

    END IF;

    RETURN total_fine;

END;
$$;