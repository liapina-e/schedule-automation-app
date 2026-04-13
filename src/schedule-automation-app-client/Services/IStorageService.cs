using System.Collections.ObjectModel;
using System.Threading.Tasks;
using schedule_automation_app_client.Models;
namespace schedule_automation_app_client.Services;

public interface IStorageService
{
    Task SaveAsync(ObservableCollection<Subject> subjects);
    Task<ObservableCollection<Subject>> LoadAsync();
}