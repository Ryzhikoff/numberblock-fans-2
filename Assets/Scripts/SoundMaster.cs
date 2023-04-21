using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMaster : MonoBehaviour {

    public AudioClip clipOne;
    public AudioClip clipOneForBig;
    public AudioClip clipTen;
    public AudioClip clipThousand;
    public AudioClip clipHundred;
    public AudioClip clipMillion;
    public AudioClip clipBillion;
    public AudioClip clipTrillion;

    public AudioClip clipQuadrillion;
    public AudioClip clipQuintillion;
    public AudioClip clipSextillion;
    public AudioClip clipSeptillion;
    public AudioClip clipOctillion;
    public AudioClip clipNonillion;
    public AudioClip clipDecillion;

    public AudioClip clipUndecillion;
    public AudioClip clipDuodecillion;
    public AudioClip clipTredecillion;
    public AudioClip clipQuattuordecillion;
    public AudioClip clipQuindecillion;
    public AudioClip clipSexdecillion;
    public AudioClip clipSeptendecillion;
    public AudioClip clipOctodecillion;
    public AudioClip clipNovemdecillion;
    public AudioClip clipVigintillion;
    public AudioClip clipUnvigintillion;
    public AudioClip clipDuovigintillion;
    public AudioClip clipTrevigintillion;
    public AudioClip clipQuattuorvigintillion;
    public AudioClip clipQuinvigintillion;
    public AudioClip clipSexvigintillion;
    public AudioClip clipSeptenvigintillion;
    public AudioClip clipOctovigintillion;
    public AudioClip clipNovemvigintillion;
    public AudioClip clipTrigintillion;
    public AudioClip clipUntrigintillion;
    public AudioClip clipDuotrigintillion;
    public AudioClip clipTrestrigintillion;
    public AudioClip clipQuattuortrigintillion;
    public AudioClip clipQuintrigintillion;
    public AudioClip clipSextrigintillion;
    public AudioClip clipSeptentrigintillion;
    public AudioClip clipOctotrigintillion;
    public AudioClip clipNovemtrigintillion;
    public AudioClip clipQuadragintillion;
    public AudioClip clipUnquadragintillion;
    public AudioClip clipDuoquadragintillion;
    public AudioClip clipTrequadragintillion;
    public AudioClip clipQuattuorquadragintillion;
    public AudioClip clipQuinquadragintillion;
    public AudioClip clipSexquadragintillion;
    public AudioClip clipSeptenquadragintillion;
    public AudioClip clipOctoquadragintillion;
    public AudioClip clipNovemquadragintillion;
    public AudioClip clipQuinquagintillion;
    public AudioClip clipUnquinquagintillion;
    public AudioClip clipDuoquinquagintillion;
    public AudioClip clipTrequinquagintillion;
    public AudioClip clipQuattuorquinquagintillion;
    public AudioClip clipQuinquinquagintillion;
    public AudioClip clipSexquinquagintillion;
    public AudioClip clipSeptenquinquagintillion;
    public AudioClip clipOctoquinquagintillion;
    public AudioClip clipNovemquinquagintillion;
    public AudioClip clipSexagintillion;
    public AudioClip clipUnsexagintillion;
    public AudioClip clipDuosexagintillion;
    public AudioClip clipTresexagintillion;
    public AudioClip clipQuattuorsexagintillion;
    public AudioClip clipQuinsexagintillion;
    public AudioClip clipSexsexagintillion;
    public AudioClip clipSeptensexagintillion;
    public AudioClip clipOctosexagintillion;
    public AudioClip clipNovemsexagintillion;
    public AudioClip clipSeptuagintillion;
    public AudioClip clipUnseptuagintillion;
    public AudioClip clipDuoseptuagintillion;
    public AudioClip clipTreseptuagintillion;
    public AudioClip clipQuattuorseptuagintillion;
    public AudioClip clipQuinseptuagintillion;
    public AudioClip clipSexseptuagintillion;
    public AudioClip clipSeptenseptuagintillion;
    public AudioClip clipOctoseptuagintillion;
    public AudioClip clipNovemseptuagintillion;
    public AudioClip clipOctogintillion;
    public AudioClip clipUnoctogintillion;
    public AudioClip clipDuooctogintillion;
    public AudioClip clipTreoctogintillion;
    public AudioClip clipQuattuoroctogintillion;
    public AudioClip clipQuinoctogintillion;
    public AudioClip clipSexoctogintillion;
    public AudioClip clipSeptoctogintillion;
    public AudioClip clipOctooctogintillion;
    public AudioClip clipNovemoctogintillion;
    public AudioClip clipNonagintillion;
    public AudioClip clipUnnonagintillion;
    public AudioClip clipDuononagintillion;
    public AudioClip clipTrenonagintillion;
    public AudioClip clipQuattuornonagintillion;
    public AudioClip clipQuinnonagintillion;
    public AudioClip clipSexnonagintillion;
    public AudioClip clipSeptennonagintillion;
    public AudioClip clipOctononagintillion;
    public AudioClip clipNovemnonagintillion;
    public AudioClip clipCentillion;
    public AudioClip clipGoogol;

    public AudioClip clipDownSound;
    public AudioClip clipBoom;
    public AudioClip mainTheme;

    public float volumeVoice;
    public float volumeBoom;
    public float volumeDown;
    public float volumeMainTheme;
    public float pitchVoiceSound;
    public float pitchMainTheme;

    public bool stopMainTheme = true;

    private static AudioSource audio1;
    private static AudioSource audio2;
    private static AudioSource audio3;

    private static AudioSource audioBoom;
    private static AudioSource audioDown;
    private static AudioSource audioMainTheme;

    private static bool auido1isStarting = false;
    private static bool auido2isStarting = false;
    private static bool auido3isStarting = false;

    private static bool isDozens = false;

    private static int counter = 1;

    //для старого режима - без генерации длинных звуков
    private bool _oldMode = false;

    /// <summary>
    /// false - по умолчанию, новый режим, с генерацией сложных звуков
    /// true - старый режим, точного воспроизведения звуков
    /// </summary>
    public bool oldMode {
        get {
            return _oldMode;
        }
        set {
            _oldMode = value;
        }
    }

    private void Start() {
        audio1 = gameObject.AddComponent<AudioSource>();
        audio2 = gameObject.AddComponent<AudioSource>();
        audio3 = gameObject.AddComponent<AudioSource>();

        audioBoom = gameObject.AddComponent<AudioSource>();
        audioBoom.clip = clipBoom;
        audioBoom.volume = volumeBoom;

        audioDown = gameObject.AddComponent<AudioSource>();
        audioDown.clip = clipDownSound;
        audioDown.volume = volumeDown;

        audioMainTheme = gameObject.AddComponent<AudioSource>();
        audioMainTheme.clip = mainTheme;
        audioMainTheme.volume = volumeMainTheme;
        audioMainTheme.loop = false;

        audioMainTheme.Play();

        //установка тональностей
        setPitchVoiceSound(pitchVoiceSound);
        setPitchMainTheme(pitchMainTheme);

        setVolumeVoice(volumeVoice);
    }

    private void Update() {


        //если установлено аудио в 1 - идем дальше
        if (audio1.clip != null) {
            //не играет в данный момент
            if (!audio1.isPlaying) {
                //не запускалась - запускаем
                if (!auido1isStarting) {
                    //print("надо запустить 1");
                    audio1.Play();
                    auido1isStarting = true;
                }
            }
            //не играет, но запускалась - удаляем
            if (!audio1.isPlaying && auido1isStarting) {
                audio1.clip = null;
                auido1isStarting = false;
            }

        }

        //не равно 0 и не играт саунд 1
        if (audio2.clip != null && !audio1.isPlaying) {
            //не играет в данный момент
            if (!audio2.isPlaying) {
                //не запускалась - запускаем
                if (!auido2isStarting) {
                    //print("надо запустить 2");
                    audio2.Play();
                    auido2isStarting = true;
                }
            }
            //не играет, но запускалась - удаляем
            if (!audio2.isPlaying && auido2isStarting) {
                audio2.clip = null;
                auido2isStarting = false;
            }
        }

        //не равно 0 и не играт саунд 1 и 2
        if (audio3.clip != null && !audio1.isPlaying && !audio2.isPlaying) {
            //не играет в данный момент
            if (!audio3.isPlaying) {
                //не запускалась - запускаем
                if (!auido3isStarting) {
                    //print("надо запустить 3");
                    audio3.Play();
                    auido3isStarting = true;
                }
            }
            //не играет, но запускалась - удаляем
            if (!audio3.isPlaying && auido3isStarting) {
                audio3.clip = null;
                auido3isStarting = false;
            }
        }

        //цикличность главной темы
        if (!audioMainTheme.isPlaying) {
            audioMainTheme.Play();
        }

        //контроль главной темы
        if (stopMainTheme) {
            if (audio1.isPlaying || audio2.isPlaying || audio3.isPlaying) {
                if (audioMainTheme.isPlaying) {
                    audioMainTheme.Pause();
                }
            } else {
                if (!audioMainTheme.isPlaying) {
                    audioMainTheme.Play();
                }
            }
        }
    }
    //установка тональности для озвучки
    public void setPitchVoiceSound(float pith) {
        audio1.pitch = pith;
        audio2.pitch = pith;
        audio3.pitch = pith;
    }
    //установка тональности для главной темы
    public void setPitchMainTheme(float pith) {
        audioMainTheme.pitch = pith;
    }
    public void playSound() {
        playSound(counter);
        counter++;
    }

    public void playSound(int counter) {

        switch (counter) {
            case 1:
                audio1.clip = clipOne;
                break;
            case 2:
                audio1.clip = clipTen;
                break;
            case 3:
                audio1.clip = clipOneForBig;
                audio2.clip = clipHundred;
                break;
            //62 - googol;
            /*case 102:
                audioMainTheme.clip = clipBoom;
                audio1.clip = clipGoogol;
                break;*/
            default:
                if (counter % 3 == 0) {
                    //print("зашли в блок для сотен counter%3 = " + counter / 3);
                    //сотни
                    audio1.clip = clipOneForBig;
                    audio2.clip = clipHundred;
                    audio3.clip = getClip((counter - 1) / 3);
                } else if (isDozens) {
                    //print("зашли в блок для десяток counter%3 = " + counter /2);
                    //десятки
                    audio1.clip = clipTen;
                    audio2.clip = getClip((counter - 1) / 3);
                    isDozens = !isDozens;
                } else {
                    //print("зашли в блок для единиц counter%3 = " + counter);
                    //единицы
                    audio1.clip = clipOneForBig;
                    audio2.clip = getClip((counter - 1) / 3);
                    isDozens = !isDozens;
                }
                break;
        }

        string t = "";
        if (audio1.clip != null) {
            t = t + " clip1: " + audio1.clip.name;
        }
        if (audio2.clip != null) {
            t = t + " clip2: " + audio2.clip.name;
        }
        if (audio3.clip != null) {
            t = t + " clip3: " + audio3.clip.name;
        }
        //print("counter: " + counter + t);
        
    }

    public void setVolumeVoice(float volume) {
        audio1.volume = volume;
        audio2.volume = volume;
        audio3.volume = volume;
    }

    private AudioClip getClip(int i) {
        switch (i) {
            case 1:
                return clipThousand;
            case 2:
                return clipMillion;
            case 3:
                return clipBillion;
            case 4:
                return clipTrillion;
            case 5:
                return clipQuadrillion;
            case 6:
                return clipQuintillion;
            case 7:
                return clipSextillion;
            case 8:
                return clipSeptillion;
            case 9:
                return clipOctillion;
            case 10:
                return clipNonillion;
            case 11:
                return clipDecillion;
            case 12:
                return clipUndecillion;
            case 13:
                return clipDuodecillion;
            case 14:
                return clipTredecillion;
            case 15:
                return clipQuattuordecillion;
            case 16:
                return clipQuindecillion;
            case 17:
                return clipSexdecillion;
            case 18:
                return clipSeptendecillion;
            case 19:
                return clipOctodecillion;
            case 20:
                return clipNovemdecillion;
            case 21:
                return clipVigintillion;
            case 22:
                return clipUnvigintillion;
            case 23:
                return clipDuovigintillion;
            case 24:
                return clipTrevigintillion;
            case 25:
                return clipQuattuorvigintillion;
            case 26:
                return clipQuinvigintillion;
            case 27:
                return clipSexvigintillion;
            case 28:
                return clipSeptenvigintillion;
            case 29:
                return clipOctovigintillion;
            case 30:
                return clipNovemvigintillion;
            case 31:
                return clipTrigintillion;
            case 32:
                return clipUntrigintillion;
            case 33:
                return clipDuotrigintillion;
            case 34:
                return clipTrestrigintillion;
            case 35:
                return clipQuattuortrigintillion;
            case 36:
                return clipQuintrigintillion;
            case 37:
                return clipSextrigintillion;
            case 38:
                return clipSeptentrigintillion;
            case 39:
                return clipOctotrigintillion;
            case 40:
                return clipNovemtrigintillion;
            case 41:
                return clipQuadragintillion;
            case 42:
                return clipUnquadragintillion;
            case 43:
                return clipDuoquadragintillion;
            case 44:
                return clipTrequadragintillion;
            case 45:
                return clipQuattuorquadragintillion;
            case 46:
                return clipQuinquadragintillion;
            case 47:
                return clipSexquadragintillion;
            case 48:
                return clipSeptenquadragintillion;
            case 49:
                return clipOctoquadragintillion;
            case 50:
                return clipNovemquadragintillion;
            case 51:
                return clipQuinquagintillion;
            case 52:
                return clipUnquinquagintillion;
            case 53:
                return clipDuoquinquagintillion;
            case 54:
                return clipTrequinquagintillion;
            case 55:
                return clipQuattuorquinquagintillion;
            case 56:
                return clipQuinquinquagintillion;
            case 57:
                return clipSexquinquagintillion;
            case 58:
                return clipSeptenquinquagintillion;
            case 59:
                return clipOctoquinquagintillion;
            case 60:
                return clipNovemquinquagintillion;
            case 61:
                return clipSexagintillion;
            case 62:
                return clipUnsexagintillion;
            case 63:
                return clipDuosexagintillion;
            case 64:
                return clipTresexagintillion;
            case 65:
                return clipQuattuorsexagintillion;
            case 66:
                return clipQuinsexagintillion;
            case 67:
                return clipSexsexagintillion;
            case 68:
                return clipSeptensexagintillion;
            case 69:
                return clipOctosexagintillion;
            case 70:
                return clipNovemsexagintillion;
            case 71:
                return clipSeptuagintillion;
            case 72:
                return clipUnseptuagintillion;
            case 73:
                return clipDuoseptuagintillion;
            case 74:
                return clipTreseptuagintillion;
            case 75:
                return clipQuattuorseptuagintillion;
            case 76:
                return clipQuinseptuagintillion;
            case 77:
                return clipSexseptuagintillion;
            case 78:
                return clipSeptenseptuagintillion;
            case 79:
                return clipOctoseptuagintillion;
            case 80:
                return clipNovemseptuagintillion;
            case 81:
                return clipOctogintillion;
            case 82:
                return clipUnoctogintillion;
            case 83:
                return clipDuooctogintillion;
            case 84:
                return clipTreoctogintillion;
            case 85:
                return clipQuattuoroctogintillion;
            case 86:
                return clipQuinoctogintillion;
            case 87:
                return clipSexoctogintillion;
            case 88:
                return clipSeptoctogintillion;
            case 89:
                return clipOctooctogintillion;
            case 90:
                return clipNovemoctogintillion;
            case 91:
                return clipNonagintillion;
            case 92:
                return clipUnnonagintillion;
            case 93:
                return clipDuononagintillion;
            case 94:
                return clipTrenonagintillion;
            case 95:
                return clipQuattuornonagintillion;
            case 96:
                return clipQuinnonagintillion;
            case 97:
                return clipSexnonagintillion;
            case 98:
                return clipSeptennonagintillion;
            case 99:
                return clipOctononagintillion;
            case 100:
                return clipNovemnonagintillion;
            case 101:
                return clipCentillion;
            default:
                return null;
        }

    }

    public void playBoom() {
        audioBoom.Play();
    }

    public void boomSoundUp() {
        boomSoundUp(0.1f);
    }
    public void boomSoundUp(float value) {
        audioBoom.volume += value;
    }

    public void boomSoundDown() {
        boomSoundDown(0.1f);
    }
    public void boomSoundDown(float value) {
        audioBoom.volume -= value;
    }

    public void playDownSound(bool play) {
        if (play) {
            audioDown.Play();
        } else {
            audioDown.Stop();
        }
    }

}
