
using CURSACH.View;
using CURSACH.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace CURSACH
{
    internal class DatabaseManager
    {



        private static string connectionString = "data source=winserver001.asuscomm.com,1433; Database=Oxana; User=admin; Password=winServer==; TrustServerCertificate=True;";




        internal static Users AuthenticateUser(string email, string password)
        {
            Users user = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
SELECT Id, Email, UserName, UserPassword, Phone, RoleId, Photo 
FROM Users 
WHERE Email = @Email AND UserPassword = @Password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new Users
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                                    Password = reader.GetString(reader.GetOrdinal("UserPassword")),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),  // Исправлено
                                    Photo = reader.IsDBNull(reader.GetOrdinal("Photo")) ? null : (byte[])reader["Photo"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при аутентификации: {ex.Message}");
            }

            return user;
        }








        public static bool DeleteTask(int taskId)
        {
            if (taskId <= 0) return false; // Проверка на корректный ID

            string query = "DELETE FROM Tasks WHERE Id = @TaskId";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TaskId", taskId);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0; // Если удалена хотя бы одна строка — успех
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении задачи: " + ex.Message);
                return false;
            }
        }






        public static void UpdateTask(Tasks task)
        {
            string query = @"UPDATE Tasks 
                     SET AssignedUserId = @AssignedUserId, 
                         ReporterId = @ReporterId, 
                         TaskDescription = @Description, 
                         Deadline = @Deadline, 
                         TaskStatusId = @StatusId, 
                         TaskPriorityId = @PriorityId,
                         TypeId = @TypeId,
                         Comments = @Comments,
                         TimeAllocated = @TimeAllocated,
                         TimeSpent = @TimeSpent
                     WHERE Id = @Id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Вывод значений перед добавлением параметров
                    Console.WriteLine($"Task ID: {task.Id}");
                    Console.WriteLine($"Description: {task.TaskDescription}");
                    Console.WriteLine($"Deadline: {task.Deadline}");
                    Console.WriteLine($"StatusId: {task.TaskStatusId}");
                    Console.WriteLine($"PriorityId: {task.TaskPriorityId}");
                    Console.WriteLine($"TypeId: {task.TypeId}");
                    Console.WriteLine($"AssignedUserId: {(task.AssignedUserId.HasValue ? task.AssignedUserId.Value.ToString() : "NULL")}");
                    Console.WriteLine($"ReporterId: {task.ReporterId}");
                    Console.WriteLine($"Comments: {task.Comments ?? "NULL"}");
                    Console.WriteLine($"TimeAllocated: {task.TimeAllocated}");
                    Console.WriteLine($"TimeSpent: {(task.TimeSpent.HasValue ? task.TimeSpent.Value.ToString() : "NULL")}");

                    // Добавляем параметры
                    command.Parameters.AddWithValue("@Description", task.TaskDescription);
                    command.Parameters.AddWithValue("@Deadline", task.Deadline);
                    command.Parameters.AddWithValue("@StatusId", task.TaskStatusId);
                    command.Parameters.AddWithValue("@PriorityId", task.TaskPriorityId);
                    command.Parameters.AddWithValue("@TypeId", task.TypeId);
                    command.Parameters.AddWithValue("@AssignedUserId", (object)task.AssignedUserId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ReporterId", task.ReporterId);
                    command.Parameters.AddWithValue("@Id", task.Id);
                    command.Parameters.AddWithValue("@Comments", string.IsNullOrEmpty(task.Comments) ? (object)DBNull.Value : task.Comments);
                    command.Parameters.AddWithValue("@TimeAllocated", task.TimeAllocated);
                    command.Parameters.AddWithValue("@TimeSpent", task.TimeSpent.HasValue ? (object)task.TimeSpent.Value : DBNull.Value);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"Rows affected: {rowsAffected}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обновлении задачи: {ex.Message}");
                    }
                }
            }
        }



        internal static void AddTask(Tasks task)
        {
            string query = @"INSERT INTO Tasks (TaskDescription, Deadline, ReporterId, TypeId, TaskStatusId, TaskPriorityId, Comments, TimeAllocated, TimeSpent)
                     VALUES (@Description, @Deadline, @ReporterId, @TypeId, @TaskStatusId, @TaskPriorityId, @Comments, @TimeAllocated, @TimeSpent)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Добавляем параметры, учитывая возможные NULL-значения
                    command.Parameters.AddWithValue("@Description", task.TaskDescription);
                    command.Parameters.AddWithValue("@Deadline", task.Deadline);
                    command.Parameters.AddWithValue("@ReporterId", task.ReporterId);
                    command.Parameters.AddWithValue("@TypeId", task.TypeId);
                    command.Parameters.AddWithValue("@TaskStatusId", 1);
                    command.Parameters.AddWithValue("@TaskPriorityId", task.TaskPriorityId);
                    command.Parameters.AddWithValue("@TimeAllocated", task.TimeAllocated);

                    command.Parameters.AddWithValue("@Comments", string.IsNullOrEmpty(task.Comments) ? (object)DBNull.Value : task.Comments);
                    command.Parameters.AddWithValue("@TimeSpent", task.TimeSpent.HasValue ? (object)task.TimeSpent.Value : DBNull.Value);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        // Обработка ошибок
                        Console.WriteLine("Ошибка при добавлении задачи в базу данных: " + ex.Message);
                    }
                }
            }
        }


        internal static bool UpdateUserProfile()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Обновленный SQL-запрос с новыми названиями полей и таблиц
                    string query = @"
            UPDATE Users 
            SET UserName = @NewName, 
                Email = @NewEmail, 
                UserPassword = @NewPassword, 
                Phone = @NewPhone
WHERE Id = @UserId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавляем параметры для новых данных
                        command.Parameters.AddWithValue("@NewName", CurrentUser.UserName);
                        command.Parameters.AddWithValue("@NewEmail", CurrentUser.Email);
                        command.Parameters.AddWithValue("@NewPassword", CurrentUser.Password); // Если есть хеширование, хешируй перед этим
                        command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);

                        // Обрабатываем возможные NULL-значения
                        if (string.IsNullOrEmpty(CurrentUser.Phone))
                            command.Parameters.AddWithValue("@NewPhone", DBNull.Value);
                        else
                            command.Parameters.AddWithValue("@NewPhone", CurrentUser.Phone);

                        

                        // Выполняем команду
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обновлении профиля пользователя: " + ex.Message);
                return false;
            }
        }



        public static bool AddUser(string name, string email, string phone, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Получаем RoleId для роли "Сотрудник"
                    string getRoleIdQuery = "SELECT RoleId FROM Roles WHERE Title = 'Сотрудник'";
                    int roleId;

                    using (SqlCommand roleCommand = new SqlCommand(getRoleIdQuery, connection))
                    {
                        object roleIdObj = roleCommand.ExecuteScalar();
                        if (roleIdObj == null)
                        {
                            Console.WriteLine("Ошибка: Роль 'Сотрудник' не найдена в базе данных.");
                            return false;
                        }
                        roleId = Convert.ToInt32(roleIdObj);
                    }

                    // Вставляем пользователя с найденным RoleId
                    string query = "INSERT INTO Users (UserName, Email, Phone, UserPassword, RoleId) VALUES (@Name, @Email, @Phone, @Password, @RoleId)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавляем параметры
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Phone", (object)phone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Password", password); // Здесь лучше хешировать пароль
                        command.Parameters.AddWithValue("@RoleId", roleId);

                        // Выполняем команду
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при добавлении пользователя: " + ex.Message);
                return false;
            }
        }
    


        internal static List<Tasks> GetMyTasks()
        {
            List<Tasks> taskList = new List<Tasks>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT Id, TaskDescription, CreatedDate, Deadline, AssignedUserId, ReporterId, 
                       TypeId, TaskStatusId, TaskPriorityId, Comments, TimeAllocated, TimeSpent 
                FROM Tasks 
                WHERE ReporterId = @UserId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tasks task = new Tasks
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    TaskDescription = reader.GetString(reader.GetOrdinal("TaskDescription")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                                    AssignedUserId = reader.IsDBNull(reader.GetOrdinal("AssignedUserId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedUserId")),
                                    ReporterId = reader.GetInt32(reader.GetOrdinal("ReporterId")),
                                    TypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                    TaskStatusId = reader.GetInt32(reader.GetOrdinal("TaskStatusId")),
                                    TaskPriorityId = reader.GetInt32(reader.GetOrdinal("TaskPriorityId")),
                                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                                    TimeAllocated = reader.GetDecimal(reader.GetOrdinal("TimeAllocated")),
                                    TimeSpent = reader.IsDBNull(reader.GetOrdinal("TimeSpent")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TimeSpent"))
                                };

                                taskList.Add(task);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении задач: {ex.Message}");
            }

            return taskList;
        }


        internal static List<Tasks> GetTasksInWork()
        {
            List<Tasks> taskList = new List<Tasks>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT t.Id, t.TaskDescription, t.CreatedDate, t.Deadline, t.AssignedUserId, t.ReporterId, 
                       t.TypeId, t.TaskStatusId, t.TaskPriorityId, t.Comments, t.TimeAllocated, t.TimeSpent 
                FROM Tasks t
                JOIN StatusesTask s ON t.TaskStatusId = s.StatusId
                WHERE t.AssignedUserId = @UserId AND s.Title = 'В работе'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tasks task = new Tasks
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    TaskDescription = reader.GetString(reader.GetOrdinal("TaskDescription")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                                    AssignedUserId = reader.IsDBNull(reader.GetOrdinal("AssignedUserId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedUserId")),
                                    ReporterId = reader.GetInt32(reader.GetOrdinal("ReporterId")),
                                    TypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                    TaskStatusId = reader.GetInt32(reader.GetOrdinal("TaskStatusId")),
                                    TaskPriorityId = reader.GetInt32(reader.GetOrdinal("TaskPriorityId")),
                                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                                    TimeAllocated = reader.GetDecimal(reader.GetOrdinal("TimeAllocated")),
                                    TimeSpent = reader.IsDBNull(reader.GetOrdinal("TimeSpent")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TimeSpent"))
                                };

                                taskList.Add(task);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении задач в работе: {ex.Message}");
            }

            return taskList;
        }


        internal static List<Tasks> GetTasksToTake()
        {
            List<Tasks> taskList = new List<Tasks>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT t.Id, t.TaskDescription, t.CreatedDate, t.Deadline, t.AssignedUserId, t.ReporterId, 
                       t.TypeId, t.TaskStatusId, t.TaskPriorityId, t.Comments, t.TimeAllocated, t.TimeSpent 
                FROM Tasks t
                JOIN StatusesTask s ON t.TaskStatusId = s.StatusId
                WHERE t.AssignedUserId = @UserId AND s.Title = 'Ожидает'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", CurrentUser.UserId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tasks task = new Tasks
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    TaskDescription = reader.GetString(reader.GetOrdinal("TaskDescription")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                                    AssignedUserId = reader.IsDBNull(reader.GetOrdinal("AssignedUserId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedUserId")),
                                    ReporterId = reader.GetInt32(reader.GetOrdinal("ReporterId")),
                                    TypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                    TaskStatusId = reader.GetInt32(reader.GetOrdinal("TaskStatusId")),
                                    TaskPriorityId = reader.GetInt32(reader.GetOrdinal("TaskPriorityId")),
                                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                                    TimeAllocated = reader.GetDecimal(reader.GetOrdinal("TimeAllocated")),
                                    TimeSpent = reader.IsDBNull(reader.GetOrdinal("TimeSpent")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TimeSpent"))
                                };

                                taskList.Add(task);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении задач в статусе 'Ожидает': {ex.Message}");
            }

            return taskList;
        }

        internal static List<Users> GetUsers()
        {
            List<Users> users = new List<Users>();

            string query = @"
        SELECT u.Id, u.Email, u.UserName, u.UserPassword, u.Phone, u.RoleId, u.Photo
        FROM Users u";  // Убрали JOIN и RoleTitle

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Users user = new Users
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                                    Password = reader.GetString(reader.GetOrdinal("UserPassword")),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")), // Оставили только RoleId
                                    Photo = reader.IsDBNull(reader.GetOrdinal("Photo")) ? null : (byte[])reader["Photo"]
                                };

                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка пользователей: {ex.Message}");
            }

            return users;
        }

        internal static List<TaskStatus> GetStatuses()
        {
            List<TaskStatus> statuses = new List<TaskStatus>();
            string query = "SELECT StatusId, Title FROM StatusesTask";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                statuses.Add(new TaskStatus
                                {
                                    StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),
                                    Title = reader.GetString(reader.GetOrdinal("Title"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении статусов задач: {ex.Message}");
            }

            return statuses;
        }

        internal static List<TaskType> GetTypes()
        {
            List<TaskType> types = new List<TaskType>();
            string query = "SELECT TypeId, Title FROM TypesTask";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                types.Add(new TaskType
                                {
                                    TypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                    Title = reader.GetString(reader.GetOrdinal("Title"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении типов задач: {ex.Message}");
            }

            return types;
        }

        internal static List<TaskPriority> GetPriorities()
        {
            List<TaskPriority> priorities = new List<TaskPriority>();
            string query = "SELECT PriorityId, Title FROM PrioritiesTask";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                priorities.Add(new TaskPriority
                                {
                                    PriorityId = reader.GetInt32(reader.GetOrdinal("PriorityId")),
                                    Title = reader.GetString(reader.GetOrdinal("Title"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении приоритетов задач: {ex.Message}");
            }
            Console.WriteLine($"получили {priorities}");

            return priorities;
        }

        internal static string GetUserRole(int userId)
        {
            string role = null;
            string query = @"
        SELECT r.Title 
        FROM Users u
        JOIN Roles r ON u.RoleId = r.RoleId
        WHERE u.Id = @UserId";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            role = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении роли пользователя: {ex.Message}");
            }

            return role;
        }

        internal static List<Tasks> GetWaitingForApproveTasks()
        {
            List<Tasks> tasks = new List<Tasks>();

            string query = @"
        SELECT Id, TaskDescription, CreatedDate, Deadline, AssignedUserId, ReporterId, 
               TypeId, TaskStatusId, TaskPriorityId, Comments, TimeAllocated, TimeSpent 
        FROM Tasks
        WHERE TaskStatusId = (SELECT StatusId FROM StatusesTask WHERE Title = 'На проверке')";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tasks task = new Tasks
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    TaskDescription = reader.GetString(reader.GetOrdinal("TaskDescription")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                                    AssignedUserId = reader.IsDBNull(reader.GetOrdinal("AssignedUserId"))
                                        ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedUserId")),
                                    ReporterId = reader.GetInt32(reader.GetOrdinal("ReporterId")),
                                    TypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                    TaskStatusId = reader.GetInt32(reader.GetOrdinal("TaskStatusId")),
                                    TaskPriorityId = reader.GetInt32(reader.GetOrdinal("TaskPriorityId")),
                                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                                    TimeAllocated = reader.GetDecimal(reader.GetOrdinal("TimeAllocated")),
                                    TimeSpent = reader.IsDBNull(reader.GetOrdinal("TimeSpent")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TimeSpent"))
                                };

                                tasks.Add(task);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка задач на проверке: {ex.Message}");
            }

            return tasks;
        }


        internal static List<Tasks> GetWaitingForAssignTasks()
        {
            List<Tasks> tasks = new List<Tasks>();

            string query = @"
        SELECT Id, TaskDescription, CreatedDate, Deadline, AssignedUserId, ReporterId, 
               TypeId, TaskStatusId, TaskPriorityId, Comments, TimeAllocated, TimeSpent 
        FROM Tasks
        WHERE AssignedUserId IS NULL";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tasks task = new Tasks
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    TaskDescription = reader.GetString(reader.GetOrdinal("TaskDescription")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                                    AssignedUserId = null, // Исполнитель не назначен
                                    ReporterId = reader.GetInt32(reader.GetOrdinal("ReporterId")),
                                    TypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                    TaskStatusId = reader.GetInt32(reader.GetOrdinal("TaskStatusId")),
                                    TaskPriorityId = reader.GetInt32(reader.GetOrdinal("TaskPriorityId")),
                                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                                    TimeAllocated = reader.GetDecimal(reader.GetOrdinal("TimeAllocated")),
                                    TimeSpent = reader.IsDBNull(reader.GetOrdinal("TimeSpent")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TimeSpent"))
                                };

                                tasks.Add(task);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка задач без исполнителя: {ex.Message}");
            }

            return tasks;
        }

        internal static List<Tasks> GetAllTasks()
        {
            List<Tasks> tasks = new List<Tasks>();

            string query = @"
        SELECT Id, TaskDescription, CreatedDate, Deadline, AssignedUserId, ReporterId, 
               TypeId, TaskStatusId, TaskPriorityId, Comments, TimeAllocated, TimeSpent 
        FROM Tasks";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tasks task = new Tasks
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    TaskDescription = reader.GetString(reader.GetOrdinal("TaskDescription")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                                    AssignedUserId = reader.IsDBNull(reader.GetOrdinal("AssignedUserId"))
                                        ? (int?)null : reader.GetInt32(reader.GetOrdinal("AssignedUserId")),
                                    ReporterId = reader.GetInt32(reader.GetOrdinal("ReporterId")),
                                    TypeId = reader.GetInt32(reader.GetOrdinal("TypeId")),
                                    TaskStatusId = reader.GetInt32(reader.GetOrdinal("TaskStatusId")),
                                    TaskPriorityId = reader.GetInt32(reader.GetOrdinal("TaskPriorityId")),
                                    Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                                    TimeAllocated = reader.GetDecimal(reader.GetOrdinal("TimeAllocated")),
                                    TimeSpent = reader.IsDBNull(reader.GetOrdinal("TimeSpent")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("TimeSpent"))
                                };

                                tasks.Add(task);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка задач: {ex.Message}");
            }

            return tasks;
        }

    }
}


