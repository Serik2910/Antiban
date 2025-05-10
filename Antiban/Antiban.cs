using System;
using System.Collections.Generic;
using System.Linq;

namespace Antiban
{
    public class Antiban
    {

        private List<AntibanResult> result = new List<AntibanResult>();
        private List<EventMessage> eventMessages = new List<EventMessage>();
        /// <summary>
        /// Добавление сообщений в систему, для обработки порядка сообщений
        /// </summary>
        /// <param name="eventMessage"></param>
        public void PushEventMessage(EventMessage eventMessage)
        {
            if (eventMessage != null)
            {
                if(result.Count > 0) {
                    //TODO
                    //Example
                    //var lastEventMessage = result[result.Count - 1];
                    DateTime targetDateTime = eventMessage.DateTime;
                    var res = from r in result
                              join e in eventMessages on r.EventMessageId equals e.Id
                              select new { SentDateTime = r.SentDateTime, EventMessageId = r.EventMessageId, DateTime = e.DateTime, Phone = e.Phone, Priority = e.Priority};
                    var result_ = res.ToList();

                    if (eventMessage.Priority == 1)
                    {
                        var eventMessageWithSameNumberWithinDay = result_.Find(x =>
                            (x.Phone == eventMessage.Phone) && (x.Priority == 1) &&
                            Math.Abs((x.DateTime - targetDateTime).TotalHours) < 24);
                        while (eventMessageWithSameNumberWithinDay != null)
                        {
                            targetDateTime = eventMessageWithSameNumberWithinDay.SentDateTime.AddHours(24);
                            eventMessageWithSameNumberWithinDay = result_.Find(x =>
                                (x.Phone == eventMessage.Phone) && (x.Priority == 1) && Math.Abs((x.SentDateTime - targetDateTime).TotalHours) < 24);
                        }
                    }
                    var eventMessageWithSameNumberWithinMinute = result_.Find(x =>
                        (x.Phone == eventMessage.Phone) &&
                        Math.Abs((x.DateTime - targetDateTime).TotalSeconds) < 60);

                    while (eventMessageWithSameNumberWithinMinute != null)
                    {
                        targetDateTime = eventMessageWithSameNumberWithinMinute.SentDateTime.AddMinutes(1);
                        eventMessageWithSameNumberWithinMinute = result_.Find(x =>
                            (x.Phone == eventMessage.Phone) &&
                            Math.Abs((x.SentDateTime - targetDateTime).TotalSeconds) < 60);
                    };

                    
                    var lastEventMessageWithDifferentNumberWithinTenSeconds = result_.Find(x =>
                        (x.Phone != eventMessage.Phone) && Math.Abs((x.SentDateTime - targetDateTime).TotalSeconds) < 10);
                    while (lastEventMessageWithDifferentNumberWithinTenSeconds != null)
                    {
                        targetDateTime = lastEventMessageWithDifferentNumberWithinTenSeconds.SentDateTime.AddSeconds(10);
                        lastEventMessageWithDifferentNumberWithinTenSeconds = result_.Find(x =>
                            (x.Phone != eventMessage.Phone) && Math.Abs((x.SentDateTime - targetDateTime).TotalSeconds) < 10);
                    }

                    eventMessage.DateTime = targetDateTime;
                }
                eventMessages.Add(eventMessage);
                result.Add(new AntibanResult()
                {
                    SentDateTime = eventMessage.DateTime,
                    EventMessageId = eventMessage.Id
                });
                result.Sort((x, y) => x.SentDateTime.CompareTo(y.SentDateTime));
            }
            //TODO
        }

        /// <summary>
        /// Вовзращает порядок отправок сообщений
        /// </summary>
        /// <returns></returns>
        public List<AntibanResult> GetResult()
        {
            return result;
        }
    }
}
