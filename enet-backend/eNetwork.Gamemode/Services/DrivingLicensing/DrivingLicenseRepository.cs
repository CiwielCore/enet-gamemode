using eNetwork.Framework.Classes;
using eNetwork.Framework.Extensions;
using eNetwork.Framework.Utils;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace eNetwork.Services.DrivingLicensing
{
    class DrivingLicenseRepository
    {
        public Task CreateDrivingLicenseCategory(DrivingLicenseCategory category)
        {
            MySqlCommand command = new MySqlCommand(@"
                INSERT INTO `driving_license_categories`
                (`character_id`, `category_class`, `date_of_issue`)
                VALUES (@characterId, @category, @dateOfIssue);
            ");

            command.Parameters.AddWithValue("@characterId", category.CharacterId);
            command.Parameters.AddWithValue("@category", (int)category.Class);
            command.Parameters.AddWithValue("@dateOfIssue", category.DateOfIssue.ToUnix());

            return ENet.Database.ExecuteAsync(command);
        }

        public Task<List<DrivingLicenseCategory>> GetDrivingLicenseCategories(int characterId)
        {
            MySqlCommand command = new MySqlCommand(@"
                SELECT * FROM `driving_license_categories`
                WHERE `character_id`=@characterId
                LIMIT @limit;
            ");

            command.Parameters.AddWithValue("@characterId", characterId);
            command.Parameters.AddWithValue("@limit", Enum.GetValues(typeof(DrivingLicenseClass)).Length);

            List<DrivingLicenseCategory> categories = new List<DrivingLicenseCategory>();
            DataTable data = ENet.Database.ExecuteRead(command);
            if (data is null || data.Rows.Count < 1)
                return Task.FromResult(categories);

            foreach (DataRow row in data.Rows)
            {
                DrivingLicenseCategory category = new DrivingLicenseCategory()
                {
                    CharacterId = Convert.ToInt32(row["character_id"]),
                    Class = (DrivingLicenseClass)Convert.ToInt32(row["category_class"]),
                    DateOfIssue = TimeUtils.UnixTimeToDateTime(Convert.ToInt64(row["date_of_issue"]))
                };

                if (categories.Exists(c => c.Class == category.Class))
                    throw new InvalidOperationException($"Duplicate categories for {characterId} category {category.Class}");

                categories.Add(category);
            }

            return Task.FromResult(categories);
        }
    }
}
