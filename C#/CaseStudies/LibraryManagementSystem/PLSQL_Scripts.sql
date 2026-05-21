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

    v_due_date DATE;

    v_old_damage_percentage INT;

    v_book_price DECIMAL;

    v_book_copy_id INT;

    v_delayed_days INT := 0;

    v_late_fine DECIMAL := 0;

    v_damage_fine DECIMAL := 0;

    v_total_fine DECIMAL := 0;

    v_increased_damage INT := 0;

BEGIN

    -- Get all required details using borrowing id

    SELECT
        br."DueDate"::DATE,
        bc."DamagePercentage",
        bk."Price",
        bc."BookCopyId"

    INTO
        v_due_date,
        v_old_damage_percentage,
        v_book_price,
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

    -- Validation

    IF NOT FOUND THEN
        RAISE EXCEPTION
            'Borrowing record not found';
    END IF;

    -- Calculate delayed days

    v_delayed_days :=
        CURRENT_DATE - v_due_date;

    IF v_delayed_days > 0 THEN

        v_late_fine :=
            v_delayed_days * 10;

    END IF;

    -- Calculate increased damage

    v_increased_damage :=
        p_new_damage_percentage -
        v_old_damage_percentage;

    -- Damage fine calculation

    IF v_increased_damage > 0 THEN

        IF p_new_damage_percentage >= 100 THEN

            v_damage_fine :=
                v_book_price;

        ELSIF v_increased_damage >= 75 THEN

            v_damage_fine := 500;

        ELSIF v_increased_damage >= 50 THEN

            v_damage_fine := 300;

        ELSIF v_increased_damage >= 25 THEN

            v_damage_fine := 100;

        END IF;

    END IF;

    -- Total fine

    v_total_fine :=
        v_late_fine + v_damage_fine;

    -- Return only fine amount

    RETURN v_total_fine;

END;
$$;

-- While going through the project, I noticed an issue in the fine addition functionality. 
-- Even though I found it after the deadline, I analyzed, debugged the problem properly and have now fixed it successfully for my learning
