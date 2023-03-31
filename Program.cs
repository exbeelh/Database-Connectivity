namespace DatabaseConnectifity;

using System.Data.SqlClient;

class Program
{
    /*
     * Variabel yang menampung properti dari database, dibuat global sehingga method yang lain dapat 
     * mengakses variabel ini.
     */
    static string ConnectionString = "Data Source=DESKTOP-FE0RC22\\SQLEXPRESS;" +
                                     "Integrated Security=True;" +
                                     "Connect Timeout=30;" +
                                     "Database = db_hr_dts2";

    // Meninisialisasikan object sqlconnection dengan nama ibject connection
    static SqlConnection connection;

    static void Main(string[] args)
    {
        /*
         * Code dibawah merupakan code untuk pengecekan apakah program telah terkoneksi dengan database
         */

        /*
        connection = new SqlConnection(ConnectionString);

        try
        {
            connection.Open();
            Console.WriteLine("Connection successfull!");
            connection.Close();
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
        }
        */

        // Method untuk mengambil seluruh data
        GetAllData();

        // Method untuk menambahkan data baru
        // InsertData("Oceania");
        
        // Method untuk mengambil data berdasarkan id
        // GetDataById(10);

        // Method untuk menghapus data berdasarkan id
        // DeleteDataById(8);

        /* 
         * Method untuk mengubah data yang terdiri dari 2 parameter 
         * (id : memilih id yang ingin dirubah, name : data yang ingin ditambahkan)
        */
        // UpdateData(3, "Asia");
    }

    // Method untuk mengambil seluruh data
    static void GetAllData()
    {
        // Menginisialisasikan objek connection dengan parameter connection string 
        connection = new(ConnectionString);

        // Agar kita dapat melakukan transaksi ataupun melihat data maka koneksi harus dibuka
        connection.Open();
        
        /* 
         * Disini saya menggunakan using. Using merupakan sebuah statement yang digunakan 
         * untuk membungkus suatu objek. Using mempunyai kemampuan untuk menghapus sebuah 
         * objek yang tidak digunakannya lagi
         */

        // Saya menggil sqlcommand dengan parameter (query, conection) sehingga code lebih ringkas
        using SqlCommand command = new("SELECT * FROM tb_region", connection);

        /* 
         * SqlDataReader merupakan class untuk membaca data dari sql server dengan value command, yang merupakan
         * query untuk mengambil seluruh data yang ada di tabel region. Sedangkan method ExecuteReader() merupakan
         * aksi dimana query di eksekusi.
         */
        using SqlDataReader reader = command.ExecuteReader();

        /*
         * Jika reader memiliki data maka data akan ditampilkan, jika reader.HasRows null maka
         * code akan mengeksekusi "Data is empty"
         */
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                Console.WriteLine("id   : " + reader[0]);
                Console.WriteLine("Name : " + reader[1]);
                Console.WriteLine("===========================");
            }
        }
        else
        {
            Console.WriteLine("Data is empty");
        }
        // reader.Close() reader ditutup kembali ketika sudah dieksekusi
        reader.Close();

        // koneksi ditutup
        connection.Close();
    }

    // Method untuk mengambil data berdasarkan id
    static void GetDataById(int id)
    {
        connection = new(ConnectionString);

        using SqlCommand command = new("SELECT * FROM tb_region WHERE id = @id", connection);

        /* 
         * Saya menggunakan method Parameters.AddWithValue agar kode lebih ringkas.
         * AddWithValue memiliki 2 parameter yaitu @target dan value
        */
        command.Parameters.AddWithValue("@id", id);
        connection.Open();

        /*
         * Jika reader memiliki data maka data akan ditampilkan, jika reader.HasRows null maka
         * code akan mengeksekusi "Data with id @id, Not Found!"
         */
        using SqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                Console.WriteLine("Id   : " + reader[0]);
                Console.WriteLine("Name : " + reader[1]);
            }
        }
        else
        {
            Console.WriteLine($"Data with id {id}, Not Found!");
        }
        reader.Close();

        connection.Close();
    }

    //TRANSACTION

    // INSERT
    // Menambahkan data dengan parameter value atau data yang akan diisi
    static void InsertData(string name)
    {
        connection = new(ConnectionString);

        connection.Open();

        // Pertama saya memanggil sqltransaction terlebih dahulu dengan menggunakan using statement
        using SqlTransaction transaction = connection.BeginTransaction();

        // Disini saya menggunakan sql command dengan 3 parameter (query, connection, transaction)
        using SqlCommand command = new("INSERT INTO tb_region VALUES (@name)", connection, transaction);

        // Menggunakan try-catch untuk mengetahui kesalahan jika code ada yang error
        try
        {
            command.Parameters.AddWithValue("@name", name);

            /* 
             * Ini merupakan sebuah condisional dimana jika command berhasil dieksekusi
             * maka command.ExecuteNonQuery akan ditambahkan 1
             */
            int result = command.ExecuteNonQuery();
            /* 
             * Transaksi akan di eksekusi dan ditampilkan di database, jika tidak menggunakan 
             * commit data memang bertambah namun data di database tidak akan muncul
             */
            transaction.Commit();

            // Jika transaksi berhasil maka akan muncul "Data added successfully!"
            if (result > 0)
            {
                Console.WriteLine("Data added successfully!");
            }
            // Jika transaksi gagal maka akan muncul "Data failed to added!"
            else
            {
                Console.WriteLine("Data failed to added!");
            }
            connection.Close();
        }

        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
    }

    // DELETE
    // Menghapus data berdasarkan value parameter id
    static void DeleteDataById(int id)
    {
        connection = new(ConnectionString);
        connection.Open();

        using SqlTransaction transaction = connection.BeginTransaction();
        
        // Query (parameter pertama) yang saya pakai merupakan query delete data berdasarkan id
        using SqlCommand command = new("DELETE tb_region WHERE id = @id", connection, transaction);
        try
        {
            command.Parameters.AddWithValue("@id", id);

            int result = command.ExecuteNonQuery();
            transaction.Commit();
            if (result >= 0)
            {
                Console.WriteLine("Data has been deleted!");
            }
            else
            {
                Console.WriteLine("Data not found!");
            }

            connection.Close();
        }
        catch (Exception e)
        {
            // Disini merupakan transction rollback, dimana jika data gagal dirubah maka data akan kembali seperti sebelumnya
            transaction.Rollback();
            Console.WriteLine(e.Message);
        }
    }

    // UPDATE
    // Mengubah data menggunakan parameter id yang dituju dan name merupakan data yang akan dirubah
    static void UpdateData(int id, string name)
    {
        connection = new(ConnectionString);
        connection.Open();

        using (SqlTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                // Query (parameter pertama) yang saya pakai merupakan query delete data berdasarkan id
                using SqlCommand command = new("UPDATE tb_region SET name = @name WHERE id = @id", connection, transaction);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", name);

                int result = command.ExecuteNonQuery();
                transaction.Commit();

                if (result >= 0)
                {
                    Console.WriteLine("Data has been updated!");
                }
                else
                {
                    Console.WriteLine("Data not found!");
                }

                connection.Close();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Console.WriteLine(e.Message);
            }
            
        }
    }

}
