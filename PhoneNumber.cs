using CashphotoWPF.BDD;
using CashphotoWPF.Configuration;
using System;
using System.Text.RegularExpressions;

namespace CashPhoto
{
    /// <summary>
    /// classe outil pour travailler sur les numéros de téléphone
    /// </summary>
    public class PhoneNumber
    {
        public Commande c;
        public enum typesNum { Mobile, Fixe, Inconnu };
        public typesNum typeNum = typesNum.Inconnu;
        private string _MobileNum = "";
        private string _FixeNum = "";
        /// <summary>
        /// cherche dans les deux champs de téléphone de la commande, détecte si c'est un num fixe ou mobile, le formatte si il est non normalisé, en cas d'échec appelle l'utilisteur au secours 
        /// </summary>
        /// <param name="commande">La commande dont on cherche à identifier le numéro de téléphone</param>
        public PhoneNumber(Commande commande)
        {
            Constante constante = Constante.GetConstante();

            if (commande.TelClientLivraison != null && !commande.TelClientLivraison.Equals(""))
                formatter(commande.TelClientLivraison);
            else if (commande.TelClientFacturation != null && !commande.TelClientFacturation.Equals(""))
                formatter(commande.TelClientFacturation);
            else
            {
                _FixeNum = constante.telephone;
                typeNum = typesNum.Fixe; //Le numéro de téléphone par défaut est un fixe
            }
        }

        private void formatter(string num)
        {
            string pattern = "\\(.+\\)";
            string newNum = Regex.Replace(num, pattern, String.Empty); //on supprime les détails entre parenthèses
            pattern = "[^\\d+]";
            newNum = Regex.Replace(newNum, pattern, String.Empty); //on formatte pour ne garder que les numéros et l'éventuel +

            if (newNum.Length == 10 && Regex.IsMatch(newNum, "^0\\d")) //format standard 0X XX XX XX XX
            {
                if (Regex.IsMatch(newNum, "^0[1-5]")) //téléphone fixe
                {
                    typeNum = typesNum.Fixe;
                    _FixeNum = newNum;
                }
                else //téléphone mobile
                {
                    if (Regex.IsMatch(newNum, "^0[67]")) //numéro de mobile, 8 et 9 sont des numéros spéciaux illégaux
                    {
                        typeNum = typesNum.Mobile;
                        _MobileNum = newNum;
                    }
                }
            }
            else
            {
                if (Regex.IsMatch(newNum, "^\\+?\\d+$"))//Si c'est un num au format +XX XX..XX (format international, difficile à vérifier), alors OK
                {
                    if (Regex.IsMatch(newNum, "^\\+"))
                    {
                        if (newNum.Length == 12)  // numéro standard +3X X XX XX XX XX, format internationnal europe avec code pays à 2 chiffres (33 pour france, 41 suisse, 49 allemagne) on ne vérifie pas en détail
                        {
                            if (Regex.IsMatch(newNum, "^\\+33[67]"))
                            {
                                typeNum = typesNum.Mobile;
                                _MobileNum = newNum;
                            }
                            if (Regex.IsMatch(newNum, "^\\+33[1-5]"))
                            {
                                typeNum = typesNum.Fixe;
                                _FixeNum = newNum;
                            }
                        }

                    }
                    else
                    {
                        if (newNum.Length == 11) //idem que au dessus mais sans le +
                        {
                            if (Regex.IsMatch(newNum, "^33[67]"))
                            {
                                typeNum = typesNum.Mobile;
                                _MobileNum = newNum;
                            }
                            else
                            if (Regex.IsMatch(newNum, "^33[1-5]"))
                            {
                                typeNum = typesNum.Fixe;
                                _FixeNum = newNum;
                            }
                           
                        }
                       
                    }
                }

            }
        }

        public string getFixe()
        {
            Constante constante = Constante.GetConstante();
            if (_MobileNum == "" && _FixeNum == "")
                _FixeNum = constante.telephone;
            return _FixeNum;
        }

        public string getMobile()
        {
            return _MobileNum;
        }
    }
}
